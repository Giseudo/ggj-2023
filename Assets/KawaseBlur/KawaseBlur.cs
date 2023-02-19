using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class KawaseBlur : ScriptableRendererFeature
{
    [System.Serializable]
    public class KawaseBlurSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material blurMaterial = null;

        [Range(2,15)]
        public int blurPasses = 1;

        [Range(1,4)]
        public int downsample = 1;
        public bool copyToFramebuffer;
        public string targetName = "_blurTexture";
    }

    public KawaseBlurSettings settings = new KawaseBlurSettings();

    class CustomRenderPass : ScriptableRenderPass
    {
        public Material blurMaterial;
        public int passes;
        public int downsample;
        public bool copyToFramebuffer;
        public string targetName;        
        string profilerTag;

        // int tmpId1;
        // int tmpId2;
        // RenderTargetIdentifier tmpRT1;
        // RenderTargetIdentifier tmpRT2;

        RTHandle _tmpRT1;
        RTHandle _tmpRT2;
        ScriptableRenderer _renderer;
        
        private RenderTargetIdentifier source { get; set; }

        public void Setup(ScriptableRenderer renderer) {
            _renderer = renderer;
        }

        public CustomRenderPass(string profilerTag)
        {
            this.profilerTag = profilerTag;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            this.source = _renderer.cameraColorTargetHandle;

            var width = cameraTextureDescriptor.width / downsample;
            var height = cameraTextureDescriptor.height / downsample;

            int tmpId1 = Shader.PropertyToID("tmpBlurRT1");
            int tmpId2 = Shader.PropertyToID("tmpBlurRT2");

            cmd.GetTemporaryRT(tmpId1, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
            cmd.GetTemporaryRT(tmpId2, width, height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);

            RenderTargetIdentifier tmpRT1 = new RenderTargetIdentifier(tmpId1);
            RenderTargetIdentifier tmpRT2 = new RenderTargetIdentifier(tmpId2);

            _tmpRT1 = RTHandles.Alloc(tmpRT1, name: "tmpBlurRT1");
            ConfigureTarget(_tmpRT1);

            _tmpRT2 = RTHandles.Alloc(tmpRT2, name: "tmpBlurRT2");
            ConfigureTarget(_tmpRT2);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;

            // first pass
            // cmd.GetTemporaryRT(tmpId1, opaqueDesc, FilterMode.Bilinear);
            cmd.SetGlobalFloat("_offset", 1.5f);
            cmd.Blit(source, Shader.PropertyToID(_tmpRT1.name), blurMaterial);

            for (var i=1; i<passes-1; i++) {
                cmd.SetGlobalFloat("_offset", 0.5f + i);
                cmd.Blit(Shader.PropertyToID(_tmpRT1.name), Shader.PropertyToID(_tmpRT2.name), blurMaterial);

                // pingpong
                var rttmp = _tmpRT1;
                _tmpRT1 = _tmpRT2;
                _tmpRT2 = rttmp;
            }

            // final pass
            cmd.SetGlobalFloat("_offset", 0.5f + passes - 1f);
            if (copyToFramebuffer) {
                cmd.Blit(Shader.PropertyToID(_tmpRT1.name), source, blurMaterial);
            } else {
                cmd.Blit(Shader.PropertyToID(_tmpRT1.name), Shader.PropertyToID(_tmpRT2.name), blurMaterial);
                cmd.SetGlobalTexture(targetName, Shader.PropertyToID(_tmpRT2.name));
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass scriptablePass;

    public override void Create()
    {
        scriptablePass = new CustomRenderPass("KawaseBlur");
        scriptablePass.blurMaterial = settings.blurMaterial;
        scriptablePass.passes = settings.blurPasses;
        scriptablePass.downsample = settings.downsample;
        scriptablePass.copyToFramebuffer = settings.copyToFramebuffer;
        scriptablePass.targetName = settings.targetName;

        scriptablePass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        scriptablePass.Setup(renderer);
        renderer.EnqueuePass(scriptablePass);
    }
}


