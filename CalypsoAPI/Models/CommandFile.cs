﻿namespace CalypsoAPI.Models
{
     /// <summary>
     /// Model for observerCommandFile.txt
     /// </summary>
    internal class CommandFile
    {
        internal string state { get; set; }
        internal string toleranceState { get; set; }
        internal string hdrPath { get; set; }
        internal string chrPath { get; set; }
        internal string fetPath { get; set; }
        internal string planPath { get; set; }
    }
}
