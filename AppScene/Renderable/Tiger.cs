using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldWind.Renderable;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Drawing;

namespace AppScene.Renderable
{
    public class Tiger:RenderableObject
    {
        Mesh mesh = null;
        Material meshMaterials;
        Texture[] meshTextures;
        Microsoft.DirectX.Direct3D.Material[] meshMaterials1;
        public Tiger(string name):base(name)
        {
        }
        public override void Initialize(DrawArgs drawArgs)
        {
           

            meshMaterials = new Material();
            meshMaterials.Ambient = System.Drawing.Color.White;		//材质如何反射环境光
            meshMaterials.Diffuse = System.Drawing.Color.White;// Color.FromArgb(127, 255, 255, 255);//材质如何反射灯光

            drawArgs.Device.Material = meshMaterials;//指定设备的材质

            ExtendedMaterial[] materials = null;
            //下句从tiger.x文件中读入3D图形(立体老虎)
            mesh = Mesh.FromFile(@"..\..\tiger.x", MeshFlags.SystemMemory,drawArgs.Device, out materials);
            if (meshTextures == null)//如果还未设置纹理，为3D图形增加纹理和材质
            {
                meshTextures = new Texture[materials.Length];//纹理数组
                meshMaterials1 = new Microsoft.DirectX.Direct3D.Material[materials.Length];//材质数组
                for (int i = 0; i < materials.Length; i++)//读入纹理和材质
                {
                    meshMaterials1[i] = materials[i].Material3D;
                    meshMaterials1[i].Ambient = meshMaterials1[i].Diffuse;
                    meshTextures[i] = TextureLoader.FromFile(drawArgs.Device,@"..\..\" + materials[i].TextureFilename);
                }
            }        
            this.isInitialized = true;
        }

        public override void Update(DrawArgs drawArgs)
        {
            if (!isInitialized && isOn)
            {
                Initialize(drawArgs);
            }
        }

        public override void Render(DrawArgs drawArgs)
        {
            if (!this.isOn || !this.isInitialized) return;
            VertexFormats format = drawArgs.Device.VertexFormat;
            FillMode currentCull = drawArgs.Device.RenderState.FillMode;
            int currentColorOp = drawArgs.Device.GetTextureStageStateInt32(0, TextureStageStates.ColorOperation);
            int zBuffer = drawArgs.Device.GetRenderStateInt32(RenderStates.ZEnable);
            try
            {
               
                drawArgs.Device.RenderState.ZBufferEnable = false;		 	//允许使用深度缓冲
                drawArgs.Device.RenderState.Ambient = System.Drawing.Color.White;//设定环境光为白色
                drawArgs.Device.Lights[0].Type = LightType.Directional;  	//设置灯光类型
                drawArgs.Device.Lights[0].Diffuse = Color.White;			//设置灯光颜色
                drawArgs.Device.Lights[0].Direction = new Vector3(0, -1, 0);	//设置灯光位置
                drawArgs.Device.Lights[0].Update();						//更新灯光设置，创建第一盏灯光
                drawArgs.Device.Lights[0].Enabled = true;					//使设置有效

                drawArgs.Device.TextureState[0].ColorOperation = TextureOperation.Modulate;
                drawArgs.Device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
                drawArgs.Device.TextureState[0].ColorArgument2 = TextureArgument.Diffuse;
                drawArgs.Device.TextureState[0].AlphaOperation = TextureOperation.Disable;
               // drawArgs.Device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;

                for (int i = 0; i < meshMaterials1.Length; i++)//Mesh中可能有多个3D图形，逐一显示
                {
                    drawArgs.Device.Material = meshMaterials1[i];//设定3D图形的材质
                    drawArgs.Device.SetTexture(0, meshTextures[i]);//设定3D图形的纹理
                    mesh.DrawSubset(i);//显示该3D图形
                }
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                drawArgs.Device.VertexFormat = format;
                drawArgs.Device.RenderState.FillMode = currentCull;
                drawArgs.Device.SetTextureStageState(0, TextureStageStates.ColorOperation, currentColorOp);
                drawArgs.Device.SetRenderState(RenderStates.ZEnable, zBuffer);
            }
           
        }
     
        public override void Dispose()
        {
            if (mesh!=null)
            {
                mesh.Dispose();
                mesh = null;
            }
            if (meshTextures!=null)
            {
               
            }
        }

        public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            throw new NotImplementedException();
        }
    }
}
