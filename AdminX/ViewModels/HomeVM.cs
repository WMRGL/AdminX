﻿using Microsoft.EntityFrameworkCore;
using ClinicalXPDataConnections.Models;

namespace AdminX.ViewModels
{
    [Keyless]
    public class HomeVM
    {        
        public string name { get; set; }
        public string staffCode { get; set; }
        public bool isLive { get; set; }
        public string notificationMessage { get; set; }
    }
}
