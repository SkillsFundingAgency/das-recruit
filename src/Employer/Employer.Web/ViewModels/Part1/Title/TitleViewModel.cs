﻿using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Title
{
    public class TitleViewModel : TitleEditModel
    {
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(TitleEditModel.NumberOfPositions),
            nameof(TitleEditModel.Title)
        };
    }
}