/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ProjectName: WorldModel.csproj
// FileName: ControlCommands.cs
// FileType: Visual C# source file
// Professor: Daniel Kopta
// Author: Kevin Matsui, Ee Jae Ahn
// Date: 11/29/2021
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    /// <summary>
    /// Class to make the ControlCommands objects
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ControlCommands
    {
        // given fields made as JsonProperties
        [JsonProperty]
        public string moving { get;  set; } = "none";
        [JsonProperty]
        public string fire { get;  set; } = "none";
        [JsonProperty]
        public Vector2D tdir { get;  set; } = new Vector2D(0,-1);
    }
}
