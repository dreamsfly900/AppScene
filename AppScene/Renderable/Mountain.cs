using System;
using WorldWind.Renderable;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using Microsoft.DirectX;
using Utility;
namespace AppScene
{
    class Mountain : RenderableObject
    {
        private CustomVertex.PositionColoredTextured[] vertices;//定义顶点变量 
        private Texture texture;//定义贴图变量 
        private Material material;//定义材质变量 

        private VertexBuffer vertexBuffer;//定义顶点缓冲变量 
        private IndexBuffer indexBuffer;//定义索引缓冲变量 
        private int[] indices;//定义索引号变量 

        private int xCount = 5, yCount = 4;//定义横向和纵向网格数目 
        private float cellHeight = 1f, cellWidth = 1f;//定义单元的宽度和长度 
        public string texturePath = @"Data\\Terrain2.BMP";//定义贴图路径 
        public string heightMapPath = @"Data\\Terrain2.BMP";//定义高度图路径 
        Bitmap bitmap = null;
        public Mountain(string name)
            : base(name)
        {
        }
        public override void Initialize(DrawArgs drawArgs)
        {
            this.isInitialized = true;
            string bitmapPath = heightMapPath;
            bitmap = new Bitmap(bitmapPath);
            xCount = (bitmap.Width - 1) / 2;
            yCount = (bitmap.Height - 1) / 2;
            cellWidth = bitmap.Width * 5 / xCount;
            cellHeight = bitmap.Height * 5 / yCount;

            vertexBuffer = new
VertexBuffer(typeof(CustomVertex.PositionColoredTextured), (xCount + 1) * (yCount + 1), drawArgs.Device, Usage.Dynamic | Usage.WriteOnly,
CustomVertex.PositionColoredTextured.Format, Pool.Default);
            vertices = new CustomVertex.PositionColoredTextured[(xCount + 1) * (yCount + 1)];//定义顶点
            indexBuffer = new IndexBuffer(typeof(int), 6 * xCount * yCount, drawArgs.Device,
Usage.WriteOnly, Pool.Default);
            indices = new int[6 * xCount * yCount];
            VertexDeclaration();//定义顶点 
            IndicesDeclaration();//定义索引缓冲 
            LoadTexturesAndMaterials(drawArgs);//导入贴图和材质 
        }
        private void VertexDeclaration()//定义顶点 
        {
            for (int i = 0; i < yCount + 1; i++)
            {
                for (int j = 0; j < xCount + 1; j++)
                {
                    Color color = bitmap.GetPixel((int)(j * cellWidth / 5), (int)(i *
cellHeight / 5));
                    float height = float.Parse(color.R.ToString()) +
float.Parse(color.G.ToString()) + float.Parse(color.B.ToString());
                    height /= 10;
                    if (i < 5 || i > 10 || j < 5 || j > 10)
                    {
                        vertices[j + i * (xCount + 1)].Position = new Vector3(i * cellHeight, height, j *
cellWidth);
                        Color col = Color.FromArgb(255, 0, 255, 255);
                        vertices[j + i * (xCount + 1)].Color = col.ToArgb();
                        vertices[j + i * (xCount + 1)].Tu = (float)j / (xCount + 1);
                        vertices[j + i * (xCount + 1)].Tv = (float)i / (yCount + 1);
                    }


                    //if (height > 20)
                    //{
                    //    Color col = Color.FromArgb(0, 0, 255, 0);
                    //    vertices[j + i * (xCount + 1)].Color = col.ToArgb();
                    //    vertices[j + i * (xCount + 1)].Tu = (float)j / (xCount + 1);
                    //    vertices[j + i * (xCount + 1)].Tv = (float)i / (yCount + 1);
                    //}
                    //else
                    //{
                    //    Color col0 = Color.DarkSlateBlue;
                    //    Color col = Color.FromArgb(128, col0.R, col0.G, col0.B);
                    //    //
                    //    vertices[j + i * (xCount + 1)].Color = col.ToArgb();
                    //    vertices[j + i * (xCount + 1)].Tu = (float)j / (xCount + 1);
                    //    vertices[j + i * (xCount + 1)].Tv = (float)i / (yCount + 1);
                    //    //vertices[j + i * (xCount + 1)].Tu = 1;
                    //    //vertices[j + i * (xCount + 1)].Tv = 1;
                    //    //vertices[j + i * (xCount + 1)].
                    //}
                }
            }

            vertexBuffer.SetData(vertices, 0, LockFlags.None);

        }
        private void IndicesDeclaration()//定义索引 
        {

            for (int i = 0; i < yCount; i++)
            {
                for (int j = 0; j < xCount; j++)
                {
                    if (i < 5 || i > 10 || j < 5 || j > 10)
                    {
                        indices[6 * (j + i * xCount)] = j + i * (xCount + 1);
                        indices[6 * (j + i * xCount) + 1] = j + (i + 1) * (xCount + 1);
                        indices[6 * (j + i * xCount) + 2] = j + i * (xCount + 1) + 1;
                        indices[6 * (j + i * xCount) + 3] = j + i * (xCount + 1) + 1;
                        indices[6 * (j + i * xCount) + 4] = j + (i + 1) * (xCount + 1);
                        indices[6 * (j + i * xCount) + 5] = j + (i + 1) * (xCount + 1)
    + 1;
                    }
                }
            }
            indexBuffer.SetData(indices, 0, LockFlags.None);
        }
        private void LoadTexturesAndMaterials(DrawArgs drawArgs)//导入贴图和材质 
        {
            material = new Material();
            material.Diffuse = Color.FromArgb(127, 255, 255, 255);
            material.Specular = Color.LightGray;
            material.SpecularSharpness = 15.0F;
            drawArgs.Device.Material = material;
            texture = TextureLoader.FromFile(drawArgs.Device, texturePath);
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
                //drawArgs.device.RenderState.CullMode = Cull.None;
                drawArgs.Device.RenderState.FillMode = FillMode.Solid;
                drawArgs.Device.RenderState.Lighting = false;

                drawArgs.Device.RenderState.DiffuseMaterialSource = ColorSource.Color1;
                drawArgs.Device.RenderState.AlphaBlendEnable = true;
                drawArgs.Device.RenderState.AlphaTestEnable = true;

                drawArgs.Device.RenderState.ReferenceAlpha = 20;
                // device.RenderState.DepthBias=0.01f;
                //device.TextureState[0].TextureTransform=;
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

                drawArgs.Device.VertexFormat = CustomVertex.PositionColoredTextured.Format;
                drawArgs.Device.SetStreamSource(0, vertexBuffer, 0);
                drawArgs.Device.Indices = indexBuffer;
                drawArgs.Device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
    (xCount + 1) * (yCount + 1), 0, indices.Length / 3);
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

        public override void Dispose()
        {
            if (vertexBuffer != null)
            {
                vertexBuffer.Dispose();
                vertexBuffer = null;
            }
            if (indexBuffer != null)
            {
                indexBuffer.Dispose();
                indexBuffer = null;
            }
        }

        public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            return false;
        }
    }
}
