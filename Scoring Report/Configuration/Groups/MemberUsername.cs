﻿namespace Scoring_Report.Configuration.Groups
{
    public class MemberUsername : IMember
    {
        public string IDType => "Username";
        
        public string Identifier { get; set; } = "";
    }
}
