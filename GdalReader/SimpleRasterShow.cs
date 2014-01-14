using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppScene;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Utility;

namespace GdalReader
{
    class SimpleRasterShow : WorldWind.Renderable.RenderableObject
    {
        private CustomVertex.PositionTextured[] vertices;// 定义顶点变量
        private Texture texture;//定义贴图变量 
        private Material material;//定义材质变量 
        public Bitmap bitmap = null;
        public SimpleRasterShow(string name)
            : base(name)
        {

        }
        public override void Initialize(DrawArgs drawArgs)
        {
            this.isInitialized = true;
            LoadTexturesAndMaterials(drawArgs);
            VertexDeclaration();
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
            if (!isInitialized || !isOn)
                return;

            VertexFormats format = drawArgs.Device.VertexFormat;
            FillMode currentCull = drawArgs.Device.RenderState.FillMode;
            int currentColorOp = drawArgs.Device.GetTextureStageStateInt32(0, TextureStageStates.ColorOperation);
            int zBuffer = drawArgs.Device.GetRenderStateInt32(RenderStates.ZEnable);
            try
            {
                drawArgs.Device.RenderState.FillMode = FillMode.Solid;
                drawArgs.Device.RenderState.Lighting = false;

                drawArgs.Device.RenderState.DiffuseMaterialSource = ColorSource.Color1;
                drawArgs.Device.RenderState.AlphaBlendEnable = true;
                drawArgs.Device.RenderState.AlphaTestEnable = true;

                drawArgs.Device.RenderState.ReferenceAlpha = 20;
                drawArgs.Device.RenderState.AlphaFunction = Compare.Greater;

                drawArgs.Device.RenderState.SourceBlend = Blend.SourceAlpha;
                drawArgs.Device.RenderState.DestinationBlend = Blend.BothInvSourceAlpha;
                drawArgs.Device.RenderState.BlendOperation = BlendOperation.Add;

                drawArgs.Device.SetTexture(0, texture);//设置贴图 
                drawArgs.Device.TextureState[0].ColorOperation = TextureOperation.Modulate;
                drawArgs.Device.TextureState[0].ColorArgument1 = TextureArgument.TextureColor;
                drawArgs.Device.TextureState[0].ColorArgument2 = TextureArgument.Current;
                drawArgs.Device.TextureState[0].AlphaOperation = TextureOperation.SelectArg2;
                drawArgs.Device.TextureState[0].AlphaArgument1 = TextureArgument.TextureColor;
                //device.TextureState[0].AlphaArgument2 = TextureArgument.Diffuse;

                drawArgs.Device.VertexFormat = CustomVertex.PositionTextured.Format;
                drawArgs.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 2, vertices);
            }
            catch (Exception ex)
            {
                Log.Write(ex);
            }
            finally
            {
                drawArgs.Device.VertexFormat = format;
                drawArgs.Device.RenderState.FillMode = currentCull;
                drawArgs.Device.SetTextureStageState(0, TextureStageStates.ColorOperation, currentColorOp);
                drawArgs.Device.SetRenderState(RenderStates.ZEnable, zBuffer);
                drawArgs.Device.Indices = null;
            }
        }

        private void VertexDeclaration1()//定义顶点1 
        {
            vertices = new CustomVertex.PositionTextured[3];
            vertices[0].Position = new Vector3(10f, 10f, 0f);
            vertices[0].Tu = 1;
            vertices[0].Tv = 0;
            vertices[1].Position = new Vector3(-10f, -10f, 0f);
            vertices[1].Tu = 0;
            vertices[1].Tv = 1;
            vertices[2].Position = new Vector3(10f, -10f, 0f);
            vertices[2].Tu = 1;
            vertices[2].Tv = 1;
        }
        private void VertexDeclaration()//定义顶点 
        {
            vertices = new CustomVertex.PositionTextured[6];
            vertices[0].Position = new Vector3(10f, 10f, 0f);
            vertices[0].Tu = 1;
            vertices[0].Tv = 0;
            vertices[1].Position = new Vector3(-10f, -10f, 0f);
            vertices[1].Tu = 0;
            vertices[1].Tv = 1;
            vertices[2].Position = new Vector3(10f, -10f, 0f);
            vertices[2].Tu = 1;
            vertices[2].Tv = 1;
            vertices[3].Position = new Vector3(-10f, -10f, 0f);
            vertices[3].Tu = 0;
            vertices[3].Tv = 1;
            vertices[4].Position = new Vector3(10f, 10f, 0f);
            vertices[4].Tu = 1;
            vertices[4].Tv = 0;
            vertices[5].Position = new Vector3(-10f, 10f, 0f);
            vertices[5].Tu = 0;
            vertices[5].Tv = 0;

        }

        private void LoadTexturesAndMaterials(DrawArgs args)//导入贴图和材质 
        {
            material = new Material();
            material.Diffuse = Color.White;
            material.Specular = Color.LightGray;
            material.SpecularSharpness = 15.0F;
            args.Device.Material = material;
            System.IO.MemoryStream memory = new System.IO.MemoryStream();
            bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
            memory.Seek(0, SeekOrigin.Begin);
            texture = TextureLoader.FromStream(args.Device, memory);
            //if (File.Exists(@"d:\temp.jpg"))
            //{
            //    File.Delete(@"d:\temp.jpg");
            //}
            //bitmap.Save(@"d:\temp.jpg");
            //texture = TextureLoader.FromFile(args.Device, @"d:\temp.jpg");
        }

        public override void Dispose()
        {
        }

        public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            return true;
            // throw new NotImplementedException();
        }
    }
}
