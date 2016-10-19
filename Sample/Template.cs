using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using VRageMath;
using VRage.Game;
//using Sandbox.ModAPI.Interfaces;
using Sandbox.Game.EntityComponents;
using VRage.Game.Components;
using VRage.Collections;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game.ModAPI.Ingame;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
//using SpaceEngineers.Game.ModAPI.Ingame;
//using Sandbox.ModAPI.Ingame;

public sealed class Program : MyGridProgram
{
#line 1 "Program"
    const string hangarDoorGroupPrefix = "[HangDoor]";
    const string airVentDepressPrefix = "[AVDepress]";
    const string airVentPressPrefix = "[AVPress]";
    const string soundBlockPrefix = "[HangDoorDep]";
    const string warningLightBackPrefix = "[HDBWL]";
    const string openCommand = "Open";
    const string closeCommand = "Close";

    List<IMyTerminalBlock> depressAirVents = new List<IMyTerminalBlock>();
    List<IMyTerminalBlock> pressAirVents = new List<IMyTerminalBlock>();
    List<IMyTerminalBlock> hangarDoor = new List<IMyTerminalBlock>();
    List<IMyTerminalBlock> hangarSoundBlocks = new List<IMyTerminalBlock>();
    List<IMyTerminalBlock> hangarWarningLightsBack = new List<IMyTerminalBlock>();

    bool working = false;
    string lastCommand = "";
    string afectedObject = "";

    void Main(string args)
    {
        Echo("=Status=");
        Echo("Working: " + working);
        Echo("LastCommend: " + lastCommand);
        Echo("AfectedObject: " + afectedObject);

        depressAirVents = GetAirVents(GridTerminalSystem, airVentDepressPrefix);
        pressAirVents = GetAirVents(GridTerminalSystem, airVentPressPrefix);
        hangarDoor = GetHangarDoor(GridTerminalSystem, hangarDoorGroupPrefix);
        hangarSoundBlocks = GetSoundBlocks(GridTerminalSystem, soundBlockPrefix);
        hangarWarningLightsBack = GetLights(GridTerminalSystem, warningLightBackPrefix);

        Echo("Depress AirVents: " + depressAirVents.Count.ToString());
        Echo("Press AirVents: " + pressAirVents.Count.ToString());
        Echo("Hangar Door Blocks: " + hangarDoor.Count.ToString());
        Echo("Sound Blocks: " + hangarSoundBlocks.Count.ToString());
        Echo("Lights: " + hangarWarningLightsBack.Count.ToString());

        if (!working && args != "")
        {
            if (args.Contains(hangarDoorGroupPrefix))
            {
                afectedObject = args;
                if (lastCommand == openCommand)
                    lastCommand = closeCommand;
                else
                    lastCommand = openCommand;

            }

            switch (lastCommand)
            {
                case openCommand:
                    StartOpen();
                    break;
                case closeCommand:
                    StartClose();
                    break;
                default:
                    ProcessTick();
                    break;
            }
        }
        else
        {
            ProcessTick();
        }
    }

    void StartOpen()
    {

        working = true;
        //      - Turn off press air vents  
        CallTerminalBlockAction(pressAirVents, "OnOff_Off");
        //      - Turn on depress air vents  
        //      - Turn depress on  
        CallTerminalBlockAction(depressAirVents, "OnOff_On");
        CallTerminalBlockAction(depressAirVents, "Depressurize_On");
        CallTerminalBlockAction(hangarWarningLightsBack, "OnOff_On");
        SoundAlarm();
        //      - Wait for depress  
        //      - Open gate  
        //      - Finish (working = false, clean state)  
    }

    void StartClose()
    {
        working = true;

        //      - Close gate  
        CallTerminalBlockAction(hangarDoor, "Open_Off");
        //      - Wait gate close  
        //      - Check all gates closed  
        //      - Turn depress off  
        //      - Finish  
    }

    void ProcessTick()
    {
        if (working)
        {
            switch (lastCommand)
            {
                case openCommand:
                    if (IsRoomDepressurised(depressAirVents))
                    {
                        Echo("Room NOT Presurised");
                        CallTerminalBlockAction(hangarDoor, "Open_On");
                        working = false;
                        afectedObject = "";

                    }
                    else
                    {
                        Echo("Room Presurised");
                    }
                    break;
                case closeCommand:

                    if (IsHangarDoorSealed(hangarDoor))
                    {
                        CallTerminalBlockAction(depressAirVents, "Depressurize_Off");
                        working = false;
                        afectedObject = "";
                        CallTerminalBlockAction(hangarWarningLightsBack, "OnOff_Off");
                    }

                    break;
                default:
                    break;
            }

        }
    }

    List<IMyTerminalBlock> GetHangarDoor(IMyGridTerminalSystem localTerminal, string groupPrefix)
    {
        List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
        List<IMyTerminalBlock> hangarDoorBlocks = new List<IMyTerminalBlock>();
        localTerminal.GetBlockGroups(groups);
        foreach (IMyBlockGroup group in groups)
        {
            if (group.Name.ToLower().Contains(groupPrefix.ToLower()))
            {
                group.GetBlocksOfType<IMyDoor>(hangarDoorBlocks);
                break;
            }
        }
        return hangarDoorBlocks;
    }

    bool IsHangarDoorSealed(List<IMyTerminalBlock> hangarDoorBlocks)
    {
        foreach (IMyDoor doorBlock in hangarDoorBlocks)
        {
            if (doorBlock.OpenRatio != 0)
                return false;
        }
        return true;
    }

    List<IMyTerminalBlock> GetAirVents(IMyGridTerminalSystem localTerminal, string prefix)
    {
        List<IMyAirVent> allVents = new List<IMyAirVent>();
        List<IMyTerminalBlock> specificVents = new List<IMyTerminalBlock>();
        GridTerminalSystem.GetBlocksOfType<IMyAirVent>(allVents);
        foreach (IMyAirVent vent in allVents)
        {
            if (vent.CustomName.ToLower().Contains(prefix.ToLower()))
                specificVents.Add(vent);
        }
        return specificVents;
    }

    List<IMyTerminalBlock> GetSoundBlocks(IMyGridTerminalSystem localTerminal, string prefix)
    {
        List<IMySoundBlock> soundBlocks = new List<IMySoundBlock>();
        List<IMyTerminalBlock> filteredSoundBlocks = new List<IMyTerminalBlock>();
        GridTerminalSystem.GetBlocksOfType<IMySoundBlock>(soundBlocks);
        foreach (IMySoundBlock soundBlock in soundBlocks)
        {
            if (soundBlock.CustomName.ToLower().Contains(prefix.ToLower()))
                filteredSoundBlocks.Add(soundBlock);
        }
        return filteredSoundBlocks;
    }

    List<IMyTerminalBlock> GetLights(IMyGridTerminalSystem localTerminal, string prefix)
    {
        List<IMyInteriorLight> lights = new List<IMyInteriorLight>();
        List<IMyTerminalBlock> filteredLights = new List<IMyTerminalBlock>();
        localTerminal.GetBlocksOfType<IMyInteriorLight>(lights);
        foreach (IMyInteriorLight light in lights)
        {
            if (light.CustomName.ToLower().Contains(prefix.ToLower()))
            {
                filteredLights.Add(light);
            }
        }
        return filteredLights;
    }

    void SoundAlarm()
    {
        foreach (IMySoundBlock sb in hangarSoundBlocks)
        {
            sb.GetActionWithName("PlaySound").Apply(sb);
        }
    }
    void CallTerminalBlockAction(List<IMyTerminalBlock> blocks, string action)
    {
        foreach (IMyTerminalBlock fb in blocks)
        {
            fb.GetActionWithName(action).Apply(fb);
        }
    }


    bool IsRoomDepressurised(List<IMyTerminalBlock> vents)
    {
        foreach (IMyAirVent vent in vents)
        {
            if (vent.GetOxygenLevel() != 0)
            {
                Echo("Oxygen Level: " + vent.GetOxygenLevel());
                return false;
            }

        }
        return true;
    }

}
