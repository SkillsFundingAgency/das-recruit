﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Esfa.Recruit.Qa.Web.ViewModels
{
    public class ReviewEditModel
    {
        [FromRoute]
        public Guid ReviewId {get;set;}

        public List<string> SelectedFieldIdentifers { get; set; } = new List<string>();

        public string ReviewerComment { get; set; }

        public bool IsRefer => SelectedFieldIdentifers.Any();
    }
}
