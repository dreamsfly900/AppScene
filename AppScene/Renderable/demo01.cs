using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WorldWind.Renderable;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX;
using System.Drawing;

namespace AppScene
{
   public class demo01 : RenderableObject
    {
        public demo01(string name):base(name)
        {

        }
        public override void Initialize(DrawArgs drawArgs)
        {
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
            if (!isInitialized || !isOn)
                return;
            CustomVertex.PositionColored[] vertices = new
CustomVertex.PositionColored[3];//定义顶点 
            vertices[0].Position = new Vector3(10f, 10f, 10f);
            vertices[0].Color = Color.Red.ToArgb();
            vertices[1].Position = new Vector3(3, 20f, 10f);
            vertices[1].Color = Color.Green.ToArgb();
            vertices[2].Position = new Vector3(12, 40f, 10f);
            vertices[2].Color = Color.Yellow.ToArgb();
            drawArgs.Device.VertexFormat = CustomVertex.PositionColored.Format;
            drawArgs.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, vertices);


            CustomVertex.PositionColored[] vertices1 = new
CustomVertex.PositionColored[3];//定义顶点 
            vertices1[0].Position = new Vector3(23f, 120f, 10f);
            vertices1[0].Color = Color.Red.ToArgb();
            vertices1[1].Position = new Vector3(13f, 90f, 10f);
            vertices1[1].Color = Color.Green.ToArgb();
            vertices1[2].Position = new Vector3(45f, 56f, 45f);
            vertices1[2].Color = Color.Yellow.ToArgb();
            drawArgs.Device.VertexFormat = CustomVertex.PositionColored.Format;
            drawArgs.Device.DrawUserPrimitives(PrimitiveType.TriangleList, 1, vertices1);

           
        }

        public override void Dispose()
        {
           // throw new NotImplementedException();
        }

        public override bool PerformSelectionAction(DrawArgs drawArgs)
        {
            return true;
           // throw new NotImplementedException();
        }
    }
}
