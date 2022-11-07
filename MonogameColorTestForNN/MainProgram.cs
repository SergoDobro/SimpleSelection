using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using NNetworkLib.BaseNetwork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonogameColorTestForNN
{
    public class MainProgram
    {
        public const int drawerSide = 800;
        const int width = 100;
        const int height = 100;
        Texture2D texture;
        Texture2D texturePoint;
        Color[] colors = new Color[width * height];
        Network network = new Network(2, 20, 20, 3);
        float learnRate = 2;
        public MainProgram(GraphicsDevice graphicsDevice)
        {
            texture = new Texture2D(graphicsDevice, width, height);
            texturePoint = new Texture2D(graphicsDevice, 1, 1);
            texturePoint.SetData(new Color[] { Color.White });
            SetTable();
        }
        public List<DataPoint> datas = new List<DataPoint>();
        Random random = new Random();
        bool hasExited = false;
        public void OnExit() => hasExited = true;
        public void SetTable()
        {
            network.ZeroLayersWeights();
            for (int i = 0; i < 1; i++)
            {
                datas.Add(new DataPoint()
                {
                    inputs = new double[] { (double)random.NextDouble(), (double)random.NextDouble() }
                ,
                    outputs = new double[] { 0, 1, 0 }
                });
            }
            for (int i = 0; i < datas.Count; i++)
            {
                if (Math.Pow(datas[i].inputs[0] - 0.5, 2) + Math.Pow(datas[i].inputs[1] - 0.5, 2) < 0.1
                    || datas[i].inputs[0] > 0.9 || datas[i].inputs[1] > 0.9
                    || datas[i].inputs[0] < 0.1 || datas[i].inputs[1] < 0.1
                    )
                {
                    datas[i].outputs = new double[] { 1, 0, 0 };
                }
            }

            new Thread(() =>
            {
                int t = 0;
                while (!hasExited)
                {

                    network.NewLearn(GetSomeData(1).Select(
                        x => new
                    DataPoint()
                        {
                            inputs = x.inputs.Select(y => y + random.NextDouble() / 100).ToArray(),
                            outputs = x.outputs
                        }).ToArray(), learnRate);
                    if (t < 0)
                    {
                        UseNetwork();
                        t = 40;
                        System.Diagnostics.Debug.WriteLine(network.Cost(datas.ToArray()));
                        learnRate = (float)network.Cost(datas.ToArray());
                        if (learnRate>1)
                            learnRate *= learnRate;
                        else
                            learnRate = (float)Math.Sqrt(learnRate);
                        learnRate = 2;
                    }
                    t--;
                }
            }).Start();
        }
        public DataPoint[] GetSomeData(double percent)
        {
            if (percent == 1)
                return datas.ToArray();
            List<DataPoint> newdatas = new List<DataPoint>();
            int from = random.Next(0, (int)(datas.Count * (1 - percent)));
            //for (int i = 0; i < datas.Length * percent; i++)
            //{
            //    newdatas.Add(datas[random.Next(0, datas.Length)]);
            //}
            for (int i = from; i < datas.Count; i++)
            {
                newdatas.Add(datas[i]);
            }
            return newdatas.ToArray();
        }

        List<Vector2> velocities = new List<Vector2>();
        List<Vector2> accelerations = new List<Vector2>();
        float t = 0;
        public void Update()
        {
            while (datas.Count > velocities.Count)
            {
                velocities.Add(new Vector2());
                accelerations.Add(new Vector2());
            }
            for (int i = 0; i < datas.Count; i++)
            {
                velocities[i] += accelerations[i];
                //velocities[i].Normalize();
                velocities[i] *= 0.99f;
                datas[i].inputs[0] += velocities[i].X / 2000f;
                datas[i].inputs[1] += velocities[i].Y / 2000f;
                if (datas[i].inputs[0] > 1)
                    datas[i].inputs[0] = 0;
                if (datas[i].inputs[0] < 0)
                    datas[i].inputs[0] = 1;
                if (datas[i].inputs[1] > 1)
                    datas[i].inputs[1] = 0;
                if (datas[i].inputs[1] < 0)
                    datas[i].inputs[1] = 1;
            }
            t += 0.01f;
            if (t>1)
            {
                for (int i = 0; i < datas.Count; i++)
                {
                    accelerations[i] = new Vector2((float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f) * 0.01f;
                }
                t = 0;
            }
        }

        ButtonState prevState;
        public void MouseAddition()
        {
            var mouseState = Mouse.GetState();
            Vector2 position = mouseState.Position.ToVector2() / drawerSide;
            if (position.X > 0 && position.X < 1
                && position.Y > 0 && position.Y < 1)
            {
                if (mouseState.LeftButton == ButtonState.Pressed)//&& prevState!= mouseState.LeftButton)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Space) && datas.Count > 1)
                    {
                        for (int i = 0; i < datas.Count; i++)
                        {
                            if (new Rectangle((int)(datas[i].inputs[0] * drawerSide)
                              , (int)(datas[i].inputs[1] * drawerSide), 10, 10).Intersects(new Rectangle((position * drawerSide).ToPoint(), new Point(1, 1))))
                            {
                                datas.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                    else if(Keyboard.GetState().IsKeyDown(Keys.W) && datas.Count > 1)
                    {
                        datas.Add(new DataPoint()
                        {
                            inputs = new double[] { position.X, position.Y }
                        ,
                            outputs = new double[] { 0, 0, 1 }
                        });
                    }
                    else
                    {
                        datas.Add(new DataPoint()
                        {
                            inputs = new double[] { position.X, position.Y }
                        ,
                            outputs = new double[] { 1, 0 , 0 }
                        });
                    }
                }
                if (mouseState.RightButton == ButtonState.Pressed)
                {
                    datas.Add(new DataPoint()
                    {
                        inputs = new double[] { position.X, position.Y }
                    ,
                        outputs = new double[] { 0, 1,0 }
                    });
                }
                if (Keyboard.GetState().IsKeyDown(Keys.R) && Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                {
                    network = new Network(2, 5, 5, 2);
                    network.ZeroLayersWeights();
                }
                if (Keyboard.GetState().IsKeyDown(Keys.Delete))
                {
                    if (datas.Count > 1)
                    {
                        datas.RemoveAt(0);
                    }
                }
            }
            prevState = mouseState.LeftButton;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            MouseAddition();
            spriteBatch.Begin();
            spriteBatch.Draw(texture, new Rectangle(0, 0, drawerSide, drawerSide), Color.White);
            foreach (var p in datas)
            {
                spriteBatch.Draw(texturePoint, new Rectangle((int)(p.inputs[0] * drawerSide)
                    , (int)(p.inputs[1] * drawerSide), 10, 10), new Color((float)p.outputs[0],
                    (float)p.outputs[1], (float)p.outputs[2]));
            }
            spriteBatch.End();
        }

        public void UseNetwork()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    double[] values = network.CalculateOutputs(new double[2] { (float)i / width, (float)j / height });
                    //if (value == 0)
                    //{
                    //    colors[j * width + i] = Color.LightGreen;
                    //}
                    //else
                    //{
                    //    colors[j * width + i] = Color.LightBlue;
                    //}
                    colors[j * width + i] = new Color((float)values[0], (float)values[1], (float)values[2]);
                }
            }
            texture.SetData(colors);
        }
    }
}
