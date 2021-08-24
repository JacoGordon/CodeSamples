using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;

namespace CS310WML
{
    enum State
    {
        FoodSearching,
        Consuming,
        BioSearching,
        Sporing,
        Dead,
        Dormant
    }
    class Bacterium
    {
        public Color bacColor;
        State bacState;
        public int refID;
        public int headX;
        public int headY;
        public int midX;
        public int midY;
        public int nodeSize;
        int headingX;
        int headingY;
        public int baseTumbleTime;
        int tumbleTime;
        int highTimeToTumble;
        int lowTimeToTumble;
        int tumbleTick;
        double speed;
        public int tailX;
        public int tailY;
        int maxEnergy;
        public int energy;
        int qsThreshold;
        int localDensity;



        public Bacterium(int refID, int headX, int headY, int midX, int midY, int r, int tumbleTime, int tumbleTick, Worldspace map)
        {
            this.refID = refID;
            this.headX = headX;
            this.headY = headY;
            this.midX = midX;
            this.midY = midY;
            this.nodeSize = r;
            this.speed = (r * 3) - 1;
            this.headingX = headX - midX;
            this.headingY = headY - midY;
            this.tailX = (headingX * -1) + midX;
            this.tailY = (headingY * -1) + midY;
            this.lowTimeToTumble = (int)(tumbleTime / 2);
            this.highTimeToTumble = (int)(tumbleTime * 2);
            this.baseTumbleTime = tumbleTime;
            this.tumbleTime = tumbleTime;
            this.tumbleTick = tumbleTick;
            this.maxEnergy = 1000;
            this.energy = this.maxEnergy;
            this.qsThreshold = 9;
            map.FillSpace(headX, headY, r, refID);
            map.FillSpace(midX, midY, r, refID);
            map.FillSpace(this.tailX, this.tailY, r, refID);
            this.bacColor = Color.OliveDrab;


        }
        public State Update(Worldspace map, Worldspace foodmap)
        {
            this.localDensity = QuorumSense(map);
            switch (this.energy)
            {
                case int n when (n > this.maxEnergy / 2):
                    if (this.localDensity > qsThreshold)
                    {
                        this.bacState = State.Consuming;
                        this.bacColor = Color.SteelBlue;
                    }
                    else
                    {
                        this.bacState = State.FoodSearching;
                        this.bacColor = Color.OliveDrab;
                    }
                    break;

                case int n when (n > 0 && n <= this.maxEnergy / 2):
                    if (this.localDensity > qsThreshold)
                    {
                        if (this.energy < 50)
                        {
                            this.bacColor = Color.SlateGray;
                            this.bacState = State.Dormant;
                        }
                        else
                        {
                            this.bacState = State.Sporing;
                            this.bacColor = Color.Goldenrod;
                        }
                    }
                    else
                    {
                        this.bacState = State.BioSearching;
                        this.bacColor = Color.DarkOrange;
                    }
                    break;

                case int n when (n <= 0):
                    bacState = State.Dead;
                    break;
            }

            switch (this.bacState)      //Time to tumble is the only behaviour the bacteria can control
            {
                case State.Consuming:
                case State.Sporing:
                    this.tumbleTime = this.lowTimeToTumble;
                    break;
                case State.FoodSearching:
                case State.BioSearching:
                    this.tumbleTime = this.highTimeToTumble;
                    break;
                case State.Dormant:
                    return bacState;
                case State.Dead:
                    Die(map);
                    return bacState;

            }
            this.energy--;
            if (this.tumbleTick >= this.tumbleTime)
            {
                this.tumbleTick = 0;        //Tumble instead of move
                int tempHeadX = this.tailX;
                int tempHeadY = this.tailY;
                this.tailX = this.headX;
                this.tailY = this.headY;
                this.headX = tempHeadX;
                this.headY = tempHeadY;
                double veloMult = Math.Sqrt(Math.Pow((this.headX - this.midX), 2) + Math.Pow((this.headY - this.midY), 2)) / speed;
                this.headingX = (int)((this.headX - this.midX) / veloMult);
                this.headingY = (int)((this.headY - this.midY) / veloMult);
            }
            else
            {
                if (!Move(map))
                {
                    this.tumbleTick = tumbleTime;
                }
                else
                {
                    this.tumbleTick++;
                }
            }
            return this.bacState;
        }

        bool Move(Worldspace map)
        {
            map.FreeSpace(this.midX, this.midY, this.nodeSize);
            map.FreeSpace(this.headX, this.headY, this.nodeSize);
            map.FreeSpace(this.tailX, this.tailY, this.nodeSize);
            int[] newpos = map.FindFreeSpace(this.headX + this.headingX, this.headY + this.headingY, this.headX, this.headY, this.nodeSize);
            if (newpos[0] != this.headX && newpos[1] != this.headY)
            {

                this.tailX = this.midX;
                this.tailY = this.midY;

                this.midX = this.headX;
                this.midY = this.headY;

                this.headX = newpos[0];
                this.headY = newpos[1];

                double veloMult = Math.Sqrt(Math.Pow((this.headX - this.midX), 2) + Math.Pow((this.headY - this.midY), 2)) / speed;
                this.headingX = (int)((this.headX - this.midX) / veloMult);
                this.headingY = (int)((this.headY - this.midY) / veloMult);
                map.FillSpace(this.midX, this.midY, this.nodeSize, this.refID);
                map.FillSpace(this.headX, this.headY, this.nodeSize, this.refID);
                map.FillSpace(this.tailX, this.tailY, this.nodeSize, this.refID);
                return true;
            }
            return false;

        }
        int QuorumSense(Worldspace map)
        {
            int cellCount = 0;
            for (int i = this.headX - 4 * nodeSize; i < this.headX + 4 * nodeSize + 1; i += nodeSize * 2)       //increases by rough distance of a bacterial node each time to reduce time to execute and keep numbers low
            {
                if (i < 0 || i > map.xMax - 1)      //Out of bounds check
                {
                    continue;
                }
                for (int j = this.headY - 4 * nodeSize; j < this.headY + 4 * nodeSize + 1; j += nodeSize * 2)
                {
                    if (j < 0 || j > map.yMax - 1)      //Out of bounds check
                    {
                        continue;
                    }
                    if (map.worldmap[i, j] > 0)
                    {
                        cellCount++;
                    }
                }
            }
            return cellCount;
        }

        public bool Eat(Worldspace map, Worldspace foodmap)
        {
            if (QuorumSense(map) > (int)qsThreshold)
            {
                int foodval = foodmap.EagerDetectFilled(this.headX, this.headY, nodeSize);
                if (foodval > 0)
                {
                    this.energy += foodval;
                    foodmap.FreeSpace(this.headX, this.headY, nodeSize);
                    if (this.energy > this.maxEnergy)
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
        public void Die(Worldspace map)
        {
            bacState = State.Dead;
            map.FreeSpace(this.midX, this.midY, this.nodeSize);
            map.FreeSpace(this.headX, this.headY, this.nodeSize);
            map.FreeSpace(this.tailX, this.tailY, this.nodeSize);

        }
    }
}
