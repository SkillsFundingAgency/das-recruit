namespace Esfa.Recruit.Shared.Web.ViewModels
{
    public class PartOnePageInfoViewModel
    {
        public const string SubmitButtonCaptionForWizard = "Save and continue";
        public const string SubmitButtonCaption = "Save and preview";
        
        public bool HasCompletedPartOne { get; set; }
        public bool HasStartedPartTwo { get; set; }
        public bool IsWizard { get; private set; }
        public bool IsNotWizard => !IsWizard;
        public string SubmitButtonText => IsWizard ? SubmitButtonCaptionForWizard : SubmitButtonCaption;        

        public void SetWizard(string requestedWizardParam = null)
        {
            if (!bool.TryParse(requestedWizardParam, out var wizard))
            {
                wizard = true;
            }

            SetWizard(wizard);
        }

        public void SetWizard(bool requestedWizardParam)
        {
            if (HasCompletedPartOne == false)
            {
                IsWizard = true;
                return;
            }

            if(HasCompletedPartOne && HasStartedPartTwo)
            {
                IsWizard = false;
                return;
            }

            IsWizard = requestedWizardParam;            
        }
    }
}
