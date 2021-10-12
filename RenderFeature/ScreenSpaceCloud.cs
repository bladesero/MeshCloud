using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenSpaceCloud : ScriptableRendererFeature
{
    [SerializeField] private Shader m_Shader;
    [SerializeField] private Material m_Material;
    public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    class CustomRenderPass : ScriptableRenderPass
    {
        private Shader m_Shader;
        private Material m_Material;

        public int soildColorID = 0;
        public ShaderTagId shaderTag = new ShaderTagId("MeshCloud");
        FilteringSettings filtering;

        private string m_ProfilerTag;
        private RenderTargetIdentifier m_Source;

        public CustomRenderPass(string profilerTag,Shader shader,Material material)
        {
            m_Shader = shader;
            m_Material = material;
            m_ProfilerTag = profilerTag;
            filtering = new FilteringSettings(RenderQueueRange.opaque);
        }

        public void Setup(RenderTargetIdentifier source)
        {
            m_Source = source;
        }

        // This method is called before executing the render pass.
        // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
        // When empty this render pass will render to the active camera render target.
        // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
        // The render pipeline will ensure target setup and clearing happens in an performance manner.
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            int ID = Shader.PropertyToID("_MeshCloudBuffer");
            RenderTextureDescriptor desc = cameraTextureDescriptor;
            desc.colorFormat = RenderTextureFormat.ARGB32;
            desc.depthBufferBits = 16;
            cmd.GetTemporaryRT(ID, desc);
            ConfigureTarget(ID);
            ConfigureClear(ClearFlag.All, Color.black);
            cmd.SetGlobalTexture("_MeshCloudBuffer", ID);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (m_Shader == null && m_Material == null)
            {
                Debug.LogErrorFormat("Execute(): Missing material. {0} render pass will not execute. Check for missing reference in the renderer resources.", GetType().Name);
                return;
            }
            var drawSetting = CreateDrawingSettings(shaderTag, ref renderingData, renderingData.cameraData.defaultOpaqueSortFlags);
            context.DrawRenderers(renderingData.cullResults, ref drawSetting, ref filtering);

            var cmd = CommandBufferPool.Get(m_ProfilerTag);

            int TempColor = Shader.PropertyToID("_TempColor");
            int ID = Shader.PropertyToID("_MeshCloudBuffer");
            int TempID = Shader.PropertyToID("_MeshCloudBuffer2");
            cmd.SetGlobalTexture("_MeshCloudBuffer2", TempID);
            RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.colorFormat = RenderTextureFormat.ARGB32;
            desc.depthBufferBits = 0;
            cmd.GetTemporaryRT(TempColor, desc);
            cmd.GetTemporaryRT(TempID, desc);
            ConfigureClear(ClearFlag.None, Color.black);
            Blit(cmd, m_Source, TempColor, m_Material, 0);

            //simple blur
            for(int i=0;i<2;i++)
            {
                Blit(cmd, ID, TempID, m_Material, 1);
                Blit(cmd, TempID, ID, m_Material, 1);
                Blit(cmd, ID, TempID, m_Material, 1);
                Blit(cmd, TempID, ID, m_Material, 1);
            }
            
            Blit(cmd, TempColor, m_Source, m_Material, 2);

            cmd.ReleaseTemporaryRT(TempColor);
            cmd.ReleaseTemporaryRT(TempID);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// Cleanup any allocated resources that were created during the execution of this render pass.
        public override void FrameCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass m_ScriptablePass;

    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass("Screen Space Cloud",m_Shader,m_Material);
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        var src = renderer.cameraColorTarget;
        m_ScriptablePass.Setup(src);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


