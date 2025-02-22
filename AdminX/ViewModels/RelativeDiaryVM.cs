﻿using ClinicalXPDataConnections.Models;
using Microsoft.AspNetCore.Mvc;

namespace AdminX.ViewModels
{
    public class RelativeDiaryVM
    {
        public RelativeDiary relativeDiary {  get; set; }
        public List<RelativeDiary> relativeDiaryList { get; set; }
        public Relative relative { get; set; }
        public Patient patient { get; set; }
    }
}
