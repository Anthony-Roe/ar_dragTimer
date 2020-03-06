using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;

namespace cl_dragTimer
{

    public class Main : BaseScript
    {
        private bool enabled = true;

        private Vector3 curPosition;
        private Vector3 lastPosition;
        private int ped;
        private double curSpeed;
        private double startTime;
        private double timer;
        private double bestTime;
        private double distance;
        private double goalEnd;
        private double goalStart;
        private bool ready;
        private string statusColor;
        public Main()
        {
            this.enabled = false;
            this.curSpeed = 0;
            this.startTime = 0;
            this.timer = 0;
            this.bestTime = 0;
            this.distance = 0;
            this.goalEnd = 400;
            this.goalStart = 0;
            this.statusColor = "transparent";
            EventHandlers["onClientResourceStart"] += new Action<string>(this.OnClientResourceStart);
            this.Tick += this.HandleTimer;
        }

        private void OnClientResourceStart(string name)
        {
            SendNuiMessage("{\"menuState\":" + this.enabled.ToString().ToLower() + "}");
            RegisterCommand("timer", new Action<int, List<object>, string>((source, args, raw) =>
                {
                    if (args.Count >= 1)
                    {
                        if (args[0].ToString().ToLower() == "enable")
                        {
                            this.enabled = !this.enabled;
                            SendNuiMessage("{\"menuState\":" + this.enabled.ToString().ToLower() + ", \"setData\":{\"curTime\":" + this.timer + ",\"bestTime\":" + this.bestTime + ",\"distance\":" + this.distance + ",\"goal\":" + this.goalEnd + "}}");
                        }
                        else if (args[0].ToString().ToLower().Equals("set"))
                        {
                            if (args.Count >= 2)
                            {
                                if (args[1].ToString().ToLower().Equals("goalstart"))
                                {
                                    if (args.Count >= 3)
                                    {
                                        this.goalStart = Convert.ToDouble(args[2]) > 0 ? Convert.ToDouble(args[2]) : 0;
                                    }
                                }
                                else if (args[1].ToString().ToLower().Equals("goalend"))
                                {
                                    if (args.Count >= 3)
                                    {
                                        this.goalEnd = Convert.ToDouble(args[2]) > 0 ? Convert.ToDouble(args[2]) : 0;
                                    }
                                }
                            }
                            else
                            {
                                TriggerEvent("chat:addMessage", new
                                {
                                    color = new[] { 0, 155, 0 },
                                    args = new[] { "[dragTimer]", "Usage: /timer set [goalStart/goalEnd] (number > 0)" }
                                });
                            }
                        }
                    }
                    else
                    {
                        TriggerEvent("chat:addMessage", new
                        {
                            color = new[] { 0, 155, 0 },
                            args = new[] { "[dragTimer]", "Usage: /timer [enable/set]" }
                        });
                    }
                }), false);
        }

        public async Task HandleTimer()
        {
            if (this.enabled == false) return;
            // get speed
            double speed = GetEntitySpeed(PlayerPedId());
            this.curSpeed = speed;

            if (this.ready && this.curSpeed >= 1)
            {
                // check if we are ready and moving
                // get current position
                this.curPosition = GetEntityCoords(PlayerPedId(), false);

                // calculate traveled distance
                double calcDistance = Math.Sqrt(Math.Pow((this.curPosition.X - this.lastPosition.X), 2) + Math.Pow((this.curPosition.Y - this.lastPosition.Y), 2) + Math.Pow((this.curPosition.Z - this.lastPosition.Z), 2));

                // TO DO: convert meters to miles
                this.distance = (this.distance + calcDistance) >= this.goalEnd ? this.goalEnd : (this.distance + calcDistance);

                // save last position
                this.lastPosition = this.curPosition;

                if (this.distance < goalStart)
                {
                    // if we havent reached starting distance save current time
                    double epoch = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                    this.startTime = epoch;
                    this.statusColor = "orange";
                }
                else if (this.distance >= this.goalStart && this.distance < this.goalEnd)
                {
                    this.statusColor = "red";
                    // if we are between start and end distance measure time
                    double epoch = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                    this.timer += epoch - this.startTime;
                    this.startTime = epoch;
                }
                else
                {
                    // if we reached end distance stop timer and save results
                    this.ready = false;
                    this.statusColor = "transparent";
                    if (this.bestTime > this.timer || this.bestTime <= 0)
                    {
                        this.bestTime = this.timer;
                    }
                }
            }
            else if (speed < 1)
            {
                // ready to start, save position, save time
                this.ready = true;
                this.statusColor = "green";
                this.lastPosition = GetEntityCoords(PlayerPedId(), false);
                this.distance = 0;
                this.timer = 0;
                this.startTime = (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
            }
            else
            {
                this.ready = false;
                this.statusColor = "transparent";
            }
            SendNuiMessage("{\"setData\":{\"curTime\":" + this.timer + ",\"bestTime\":" + this.bestTime + ",\"distance\":" + this.distance + ",\"goal\":" + this.goalEnd + ",\"statusColor\":\"" + this.statusColor + "\"}}");
            await Delay(1);
        }
    }
}
