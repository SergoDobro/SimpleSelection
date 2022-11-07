using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NNetworkLib.SelectionNetwork;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleMap
{
    internal class MainGame
    {
        public Map map;
        public MainGame()
        {
            map = new Map();
        }
        public void Load(GraphicsDevice graphicsDevice)
        {
            Creature.texture = new Texture2D(graphicsDevice, 1, 1);
            Creature.texture.SetData(new Color[] { Color.White });
            for (int i = 0; i < 100000; i++)
            {
                map.Step();
            }
        }
        double time = 0;
        public void Update(GameTime gameTime)
        {
            time += gameTime.ElapsedGameTime.TotalSeconds;
            map.Step();
            if (time > 10)
            {
                for (int i = 0; i < 200000; i++)
                {
                    map.Step();
                }
                time = 0;
            }
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            map.Draw(spriteBatch);
        }
    }
    public class Map
    {
        public Map()
        {
            map = new int[width, height];
            for (int i = 0; i < width; i++)
            {
                map[i, 0] = 3;
                map[i, height-1] = 3;
            }
            for (int i = 0; i < height; i++)
            {
                map[0, i] = 3;
                map[width - 1, i] = 3;
            }
            for (int i = 0; i < 30; i++)
            {

                map[random.Next(1, width - 1), random.Next(1, height - 1)] = 1;
            }
            creatures = new List<Creature>();
            for (int i = 0; i < count; i++)
            {
                creatures.Add(new Creature() { position = new Point(width/2, height/2)});
            }
            Creature.map = this;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (map[i, j] == 1)
                        spriteBatch.Draw(Creature.texture, new Rectangle(new Point(i, j) * Creature.size, Creature.size), Color.Green);
                    else if (map[i, j] == 2)
                        spriteBatch.Draw(Creature.texture, new Rectangle(new Point(i, j) * Creature.size, Creature.size), Color.Red);
                    else if (map[i, j] == 3)
                        spriteBatch.Draw(Creature.texture, new Rectangle(new Point(i, j) * Creature.size, Creature.size), Color.DarkRed);
                    else if (map[i, j] == 4)
                        spriteBatch.Draw(Creature.texture, new Rectangle(new Point(i, j) * Creature.size, Creature.size), Color.LightBlue);
                }
            }
            foreach (var c in creatures)
                if (c.isAlive)
                    c.Draw(spriteBatch);
        }
        public void Step()
        {
            iterationLength++;
            if (random.NextDouble() < 0.04)
            {
                map[random.Next(1, width - 1), random.Next(1, height - 1)] = 1;
            }
            if (random.NextDouble() < 0.03)
            {
                map[random.Next(1, width - 1), random.Next(1, height - 1)] = 2;
            }
            if (random.NextDouble() < 0.01)
            {
                map[random.Next(1, width - 1), random.Next(1, height - 1)] = 4;
            }
            int alive = 0;
            for (int i = 0; i < creatures.Count; i++)
            {
                if (creatures[i].isAlive)
                {
                    creatures[i].Act();
                    alive++;
                }
            }
            if (alive<1)
            {
                Restart();
            }
        }
        
        public double[] GetData(Point position)
        {
            double[] res = new double[Creature.totalparamCount];
            for (int i = position.X - 1; i >= 0; i--)
            {
                int what = map[i, position.Y];
                res[0] = (position.X - i)/100f;
                if (what != 0)
                {
                    res[what] = 1;
                    break;
                }
            }
            for (int i = position.X + 1; i < width; i++)
            {
                int what = map[i, position.Y];
                res[Creature.paramCount] = (i - position.X)/ 100f;
                if (what != 0)
                {
                    res[what+Creature.paramCount] = 1;
                    break;
                }
            }

            for (int i = position.Y - 1; i >= 0; i--)
            {
                int what = map[position.X, i];
                res[Creature.paramCount*2] = (position.Y- i)/100f;
                if (what != 0)
                {
                    res[what + Creature.paramCount * 2] = 1;
                    break;
                }
            }
            for (int i = position.Y + 1; i < height; i++)
            {
                int what = map[position.X, i];
                res[Creature.paramCount * 3] = (i- position.Y)/ 100f;
                if (what != 0)
                {
                    res[what + Creature.paramCount * 3] = 1;
                    break;
                }
            }
            return res;
        }
        Random random = new Random();
        public int width = 200;
        public int height = 200;
        int[,] map;
        int count = 16;
        List<Creature> creatures;
        //2 - death
        //1 - food
        List<int> iterationsLengths = new List<int>();
        int iterationLength = 0;
        int iteration = 0;
        public void Move(Creature creature, double answer)
        {
            if (answer < 4)
            {
                Point delta = new Point(0,0);
                switch (answer)
                {
                    case 0:
                        delta = new Point(-1, 0);
                        break;
                    case 1:
                        delta = new Point(0, -1);
                        break;
                    case 2:
                        delta = new Point(1, 0);
                        break;
                    case 3:
                        delta = new Point(0, 1);
                        break;
                    default:
                        break;
                }
                for (int i = 0; i < creature.rotation; i++)
                {
                    delta = new Point(-delta.Y, - delta.X);
                }
                int tile = map[creature.position.X + delta.X, creature.position.Y + delta.Y];
                if (tile != 3)
                {
                    if (tile == 1)
                    {
                        creature.Eat();
                        map[random.Next(1, width - 1), random.Next(1, height - 1)] = 1;
                    }
                    else if (tile == 2)
                    {
                        creature.Damage(5);
                    }
                    else if (tile == 4)
                    {
                        creature.EatABuster();
                    }
                    else if (tile == 5)
                    {
                        creatures.Where(x => x.position == creature.position + delta).FirstOrDefault().daysleft -= 50;
                        creature.daysleft += 50;
                    }
                    map[creature.position.X, creature.position.Y] = 0;
                    map[creature.position.X + delta.X, creature.position.Y + delta.Y] = 5;
                    creature.position += delta;
                }
                else
                {
                    creature.DamageBorder();
                    creature.daysAlive /= 4;
                }
            }
            if (answer == 5)
                creature.rotation = (creature.rotation + 1) % 4;
            else if(answer == 6)
            {
                creature.rotation--;
                if (creature.rotation < 0) creature.rotation = 3;
            }
            else if(answer == 7)
            {
                if (creature.daysleft>10)
                {
                    Spread(creature);
                    creature.daysleft /= 2;
                }
            }
        }
        internal void CreatureDied(Creature creature)
        {
            map[creature.position.X, creature.position.Y] = 0;
        }
        public void Restart()
        {
            iteration++;
            iterationsLengths.Add(iterationLength);
            System.Diagnostics.Debug.WriteLine(iteration+": "+iterationLength);
            iterationLength = 0;
            var creat = creatures.OrderByDescending(x => x.daysAlive).ToArray();
            List<Creature> creatList = new List<Creature>();
            for (int i = 0; i < count; i++)
            {
                creatList.Add(creat[i].Clone());
                creatList.Add(creat[i].Clone());
                creatList.Last().Mutate(0.01, 0.01);
                creatList.Add(creat[i].Clone());
                creatList.Last().Mutate(0.5, 1);
                creatList.Add(creat[i].Clone());
                creatList.Last().Mutate(0.9, 2);
            }
            for (int i = 0; i < 4; i++)
            {
                creatList.Add(creat[i].Clone());
                creatList.Add(creat[i].Clone());
                creatList.Add(creat[i].Clone());
            }
            for (int i = 0; i < 3; i++)
            {
                creatList.Add(creat[0].Clone());
                creatList.Last().Mutate(0.5, 1);
                creatList.Add(creat[0].Clone());
            }
            for (int i = 0; i < creatList.Count; i++)
            {
                creatList[i].position = new Point(random.Next(1, width - 1), random.Next(1, height - 1));
            }
            creatures=null;
            creatures = creatList;
        }

        public void Spread(Creature creature)
        {
            if (random.NextDouble()<0.1)
            {
                creatures.Add(creature.Clone());
                creatures.Last().Mutate(0.9, 2);
                creatures.Last().position = creature.position;
                creatures.Last().daysleft = creature.daysleft / 2;
            }
        }
    }
    public class Creature
    {
        private Network_Selection networkSelection;
        public static Texture2D texture;
        public static Point size = new Point(5,5);
        public static Map map;

        public Point position;
        public int daysAlive = 0;
        public int daysleft = 50;
        public int rotation = 0;
        public bool isAlive = true;
        public const int paramCount = 6;
        public const int totalparamCount = 24;
        public const int additionalparamCount = 3;
        public Creature()
        {
            networkSelection = new Network_Selection(totalparamCount + additionalparamCount, totalparamCount , totalparamCount / 2, 8);
            networkSelection.ZeroLayersWeights();
        }
        public Creature(Network_Selection networkSelectionOld)
        {
            networkSelection = networkSelectionOld.Clone();
        }
        public void Act()
        {
            var state = applyRotation(map.GetData(position));
            var answer = networkSelection.CalculateOutput(state);
            /*
            0 - go left
            1 - go top
            2 - go right
            3 - go bottom
            4 - stay
            5 - rotate right
            6 - rotate left
            7 - rotate left
             */
            map.Move(this, answer);
            daysleft--;
            daysAlive++;
            if (daysleft<0 && isAlive)
                Die();
        }
        public double[] applyRotation(double[] inputNoRotation)
        {
            double[] rotated = new double[inputNoRotation.Length + additionalparamCount];
            for (int i = 0; i < inputNoRotation.Length - additionalparamCount; i++)
            {
                rotated[(i + rotation * paramCount) % totalparamCount] = inputNoRotation[i];
            }
            rotated[rotated.Length - 3] = 1 - 100f / daysleft;
            rotated[rotated.Length - 2] = Math.Abs(position.X - map.width/2) /map.width;
            rotated[rotated.Length - 1] = Math.Abs(position.Y - map.height / 2) / map.height;
            return rotated;
        }
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(texture, new Rectangle(position*size, size), Color.Blue);
        }
        internal void Die()
        {
            isAlive = false;
            map.CreatureDied(this);
        }
        internal void Eat()
        {
            daysleft+=20;
        }

        public Creature Clone()
        {
            return new Creature(networkSelection);
        }
        public void Mutate(double chance, double rate)
        {
            networkSelection.Mutate(chance, rate);
        }

        internal void DamageBorder()
        {
            Die();
        }
        internal void Damage(int dmg)
        {
            daysleft -= dmg;
        }

        internal void EatABuster()
        {
            daysleft += 100;
        }
    }
}
