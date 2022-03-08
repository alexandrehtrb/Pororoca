using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using ReactiveUI;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.ViewModels
{
    public sealed class CollectionVariablesViewModel : CollectionOrganizationItemViewModel
    {
        #region COLLECTION VARIABLES

        public ObservableCollection<VariableViewModel> Variables { get; }
        public VariableViewModel? SelectedVariable { get; set; }
        public ReactiveCommand<Unit, Unit> AddNewVariableCmd { get; }
        public ReactiveCommand<Unit, Unit> RemoveSelectedVariableCmd { get; }

        #endregion

        public CollectionVariablesViewModel(ICollectionOrganizationItemParentViewModel parentVm,
                                            PororocaCollection col) : base(parentVm, col.Name)
        {
            Variables = new(col.Variables.Select(v => new VariableViewModel(v)));
            AddNewVariableCmd = ReactiveCommand.Create(AddNewVariable);
            RemoveSelectedVariableCmd = ReactiveCommand.Create(RemoveSelectedVariable);
        }

        #region COLLECTION ORGANIZATION

        protected override void CopyThis() =>
            throw new NotImplementedException();

        #endregion
        
        #region COLLECTION VARIABLES

        private void AddNewVariable() =>
            Variables.Add(new(true, string.Empty, string.Empty, false));

        private void RemoveSelectedVariable()
        {
            if (SelectedVariable != null)
            {
                Variables.Remove(SelectedVariable);
                SelectedVariable = null;
            }
            else if (Variables.Count == 1)
            {
                Variables.Clear();
            }
        }

        public IEnumerable<PororocaVariable> ToVariables() =>
            Variables.Select(v => v.ToVariable());
        
        #endregion
    }
}