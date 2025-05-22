using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class RewindFeature : ScriptableRendererFeature {
    [SerializeField]
    private Material m_material;

    private RewindRenderPass m_scriptablePass;

    public override void Create() {
        m_scriptablePass = new RewindRenderPass(m_material);
        m_scriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
#if UNITY_EDITOR
        // Skip scene view camera rendering
        if (renderingData.cameraData.isSceneViewCamera) {
            return;
        }
#endif
        renderer.EnqueuePass(m_scriptablePass);
    }

    class RewindRenderPass : ScriptableRenderPass {
        private Material m_material;
        private RTHandle m_rewindTexture;

        public RewindRenderPass(Material material)
            : base() {
            m_material = material;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            descriptor.width = renderingData.cameraData.camera.scaledPixelWidth;
            descriptor.height = renderingData.cameraData.camera.scaledPixelHeight;

            // Allocate RTHandle for the intermediate "Rewind" texture
            m_rewindTexture = RTHandles.Alloc(descriptor);
            m_material.SetFloat("_UnscaledTime", Time.unscaledTime);

            // Optional: Set other material properties here if necessary.
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler("Rewind Pass"))) {
                // Get RTHandles for source and destination from camera data
                RTHandle source = renderingData.cameraData.renderer.cameraColorTargetHandle;

                // Perform first Blit pass: source -> rewind texture
                Blit(cmd, source, m_rewindTexture, m_material);

                // Perform second Blit pass: rewind texture -> source
                Blit(cmd, m_rewindTexture, source);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) {
            // Release the RTHandle properly
            RTHandles.Release(m_rewindTexture);
        }
    }
}