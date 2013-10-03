using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Galactic_Engine
{
    //Credits to http://thver.blogspot.nl/2012/07/how-to-create-sphere-programmatically.html
    class Sphere
    {
        VertexPositionColor[] baseVertices;
        VertexPositionColor[] vertices;
        VertexBuffer vbuffer;
        short[] indices;
        IndexBuffer ibuffer;
        float radius;
        int nvertices, nindices;
        BasicEffect effect;
        GraphicsDevice graphicd;
        Color color;

        public Sphere(float Radius, GraphicsDevice graphics, Color col, short resolution)
        {
            color = col;
            radius = Radius;
            graphicd = graphics;
            effect = new BasicEffect(graphicd);
            nvertices = resolution * resolution; // 90 vertices in a circle, 90 circles in a sphere
            nindices = resolution * resolution * 6;
            vbuffer = new VertexBuffer(graphics, typeof(VertexPositionNormalTexture), nvertices, BufferUsage.WriteOnly);
            ibuffer = new IndexBuffer(graphics, IndexElementSize.SixteenBits, nindices, BufferUsage.WriteOnly);
            createspherevertices(resolution);
            createindices(resolution);
            vbuffer.SetData<VertexPositionColor>(vertices);
            ibuffer.SetData<short>(indices);
            effect.VertexColorEnabled = true;
        }
        void createspherevertices(short res)
        {
            baseVertices = new VertexPositionColor[nvertices];
            vertices = new VertexPositionColor[nvertices];
            Vector3 center = new Vector3(0, 0, 0);
            Vector3 rad = new Vector3((float)Math.Abs(radius), 0, 0);
            for (int x = 0; x < res; x++) //90 circles, difference between each is 4 degrees
            {
                float difx = 360.0f / res;
                for (int y = 0; y < res; y++) //90 vertices, difference between each is 4 degrees 
                {
                    float dify = 360.0f / res;
                    Matrix zrot = Matrix.CreateRotationZ(MathHelper.ToRadians(y * dify)); //rotate vertex around z
                    Matrix yrot = Matrix.CreateRotationY(MathHelper.ToRadians(x * difx)); //rotate circle around y
                    Vector3 point = Vector3.Transform(Vector3.Transform(rad, zrot), yrot);//transformation

                    baseVertices[x + y * res] = new VertexPositionColor(point, color);
                    vertices[x + y * res] = new VertexPositionColor(point, color);
                }
            }
        }
        void createindices(short res)
        {
            indices = new short[nindices];
            int i = 0;
            for (int x = 0; x < res; x++)
            {
                for (int y = 0; y < res; y++)
                {
                    int s1 = x == res - 1 ? 0 : x + 1;
                    int s2 = y == res - 1 ? 0 : y + 1;
                    short upperLeft = (short)(x * res + y);
                    short upperRight = (short)(s1 * res + y);
                    short lowerLeft = (short)(x * res + s2);
                    short lowerRight = (short)(s1 * res + s2);
                    indices[i++] = upperLeft;
                    indices[i++] = upperRight;
                    indices[i++] = lowerLeft;
                    indices[i++] = lowerLeft;
                    indices[i++] = upperRight;
                    indices[i++] = lowerRight;
                }
            }
        }
        public void Draw(Camera cam, Vector3 location, Matrix World) // the camera class contains the View and Projection Matrices
        {
            effect.View = cam.ViewMatrix;
            effect.Projection = cam.ProjectionMatrix;
            effect.World = World;
            graphicd.RasterizerState = new RasterizerState() { FillMode = FillMode.Solid }; // Wireframe as in the picture
            for (int i = 0; i < nvertices; i++)
            {
                vertices[i].Position = baseVertices[i].Position + location;
            }

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicd.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.TriangleList, vertices, 0, nvertices, indices, 0, indices.Length / 3);
            }
        }
    }
}
