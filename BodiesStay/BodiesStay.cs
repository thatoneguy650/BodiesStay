using RDR2;
using RDR2.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BodiesStay : Script
{
    private int GameTimeLastCheckedPeds;
    private float DistanceToDespawn = 2000f;
    private int TimeToDespawn = 30000;
    private List<RDRPed> RDRPeds = new List<RDRPed>();

    public BodiesStay()
    {
        Tick += OnTick;
        Interval = 1;
        Initialize();
    }
    private void Initialize()
    {
       // ReadCreateSettings();
    }
    private void OnTick(object sender, EventArgs e)
    {
        CheckTargetting();

        if (Game.GameTime - GameTimeLastCheckedPeds >= 3000)
        {
            CheckPeds();
            GameTimeLastCheckedPeds = Game.GameTime;
        }
    }
    private void CheckPeds()
    {
        RDRPeds.RemoveAll(x => !x.GamePed.Exists());
        foreach (RDRPed MyRDRPed in RDRPeds.Where(x => x.GamePed.IsDead))
        {
            bool InRange = MyRDRPed.GamePed.IsInRangeOf(Game.Player.Character.Position, DistanceToDespawn);
            if (InRange)
            {
                if (!MyRDRPed.GamePed.IsPersistent)
                {
                    MyRDRPed.GamePed.IsPersistent = true;
                    MyRDRPed.ToRemove = false;
                    MyRDRPed.MarkedPersistent = true;

                    WriteToLog("CheckPeds", string.Format("Made Persisitent {0}", MyRDRPed.GamePed.Handle));
                }
            }
            else
            {
                if (MyRDRPed.GameTimeDied > 0 && Game.GameTime >= TimeToDespawn + MyRDRPed.GameTimeDied)
                {
                    if (MyRDRPed.GamePed.IsPersistent)
                    {
                        MyRDRPed.GamePed.IsPersistent = false;
                        MyRDRPed.ToRemove = true;
                        MyRDRPed.MarkedPersistent = false;
                        WriteToLog("CheckPeds", string.Format("Not Persisitent Time + Distance {0}", MyRDRPed.GamePed.Handle));
                    }
                }
            }        
        }
        RDRPeds.RemoveAll(x => x.ToRemove);
    }
    public void CheckTargetting()
    {
        Entity myEnt = GetTargetedEntity();
        if (myEnt != null)
        {
            if (myEnt is Ped)
            {
                Ped MyPed = (Ped)myEnt;
                if (!RDRPeds.Any(x => x.GamePed == MyPed))
                {
                    if(MyPed.IsDead)
                    {
                        RDRPeds.Add(new RDRPed(MyPed,Game.GameTime));
                    }
                    else
                    {
                        RDRPeds.Add(new RDRPed(MyPed));
                    }
                    WriteToLog("CheckTargetting", string.Format("Added Ped {0}", MyPed.Handle));
                    //RDR2.UI.Screen.ShowSubtitle(string.Format("Added Ped {0}", MyPed.Handle));
                }
            }
        }
    }
    public Entity GetTargetedEntity()
    {
        var entityArg = new OutputArgument();

        if (Function.Call<bool>(Hash.GET_ENTITY_PLAYER_IS_FREE_AIMING_AT, Game.Player, entityArg))
        {
            return Entity.FromHandle(entityArg.GetResult<int>());
        }
        return null;
    }
    private void ReadCreateSettings()
    {
        var MyIni = new IniFile("scripts\\BodiesStay.ini");
        //if (!MyIni.KeyExists("AIWeaponDamageModifier"))
        //    MyIni.Write("AIWeaponDamageModifier", AIWeaponDamageModifier.ToString());
        //else
        //    AIWeaponDamageModifier = float.Parse(MyIni.Read("AIWeaponDamageModifier"));

        //if (!MyIni.KeyExists("AIMeleeDamageModifer"))
        //    MyIni.Write("AIMeleeDamageModifer", AIMeleeDamageModifer.ToString());
        //else
        //    AIMeleeDamageModifer = float.Parse(MyIni.Read("AIMeleeDamageModifer"));

        //if (!MyIni.KeyExists("PlayerWeaponDamageModifier"))
        //    MyIni.Write("PlayerWeaponDamageModifier", PlayerWeaponDamageModifier.ToString());
        //else
        //    PlayerWeaponDamageModifier = float.Parse(MyIni.Read("PlayerWeaponDamageModifier"));

        //if (!MyIni.KeyExists("PlayerMeleeDamageModifier"))
        //    MyIni.Write("PlayerMeleeDamageModifier", PlayerMeleeDamageModifier.ToString());
        //else
        //    PlayerMeleeDamageModifier = float.Parse(MyIni.Read("PlayerMeleeDamageModifier"));

        //if (!MyIni.KeyExists("PlayerHealthRechargeMultiplier"))
        //    MyIni.Write("PlayerHealthRechargeMultiplier", PlayerHealthRechargeMultiplier.ToString());
        //else
        //    PlayerHealthRechargeMultiplier = float.Parse(MyIni.Read("PlayerHealthRechargeMultiplier"));

        //if (!MyIni.KeyExists("EnableKey"))
        //    MyIni.Write("EnableKey", EnableKey.ToString());
        //else
        //    EnableKey = (Keys)Enum.Parse(typeof(Keys), MyIni.Read("EnableKey"), true);

    }
    private void WriteToLog(String ProcedureString, String TextToLog)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ": " + ProcedureString + ": " + TextToLog + System.Environment.NewLine);
        File.AppendAllText("scripts\\BodiesStay\\" + "log.txt", sb.ToString());
        sb.Clear();
    }
    internal class RDRPed
    {
        public Ped GamePed;
        public int GameTimeDied = 0;
        public bool isDead = false;
        public bool ToRemove = false;
        public bool MarkedPersistent = false;

        public RDRPed(Ped MyPed)
        {
            GamePed = MyPed;
        }
        public RDRPed(Ped MyPed, int TimeDied)
        {
            GamePed = MyPed;
            GameTimeDied = TimeDied;
        }
    }
}

