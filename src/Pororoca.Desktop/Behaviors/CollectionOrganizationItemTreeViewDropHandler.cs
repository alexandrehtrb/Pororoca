using System.Collections;
using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Pororoca.Desktop.HotKeys;
using Pororoca.Desktop.ViewModels;
using Pororoca.Desktop.Views;

namespace Pororoca.Desktop.Behaviors;

public sealed class CollectionOrganizationItemTreeViewDropHandler : BaseTreeViewDropHandler
{
    protected override (bool Valid, bool WillSourceItemBeMovedToDifferentParent) Validate(TreeView tv, DragEventArgs e, object? sourceContext, object? targetContext, bool bExecute)
    {
        if (sourceContext is not CollectionOrganizationItemViewModel sourceItem
         || targetContext is not CollectionsGroupViewModel
         || tv.GetVisualAt(e.GetPosition(tv)) is not Control targetControl
         || targetControl.DataContext is not CollectionOrganizationItemViewModel targetItem
         || sourceItem == targetItem
         || sourceItem.Parent == targetItem
         || KeyboardShortcuts.Instance.HasMultipleItemsSelected)
        // moving multiple items is disabled because 
        // when an item is clicked to be dragged (whilst pressing Ctrl),
        // it becomes unselected and won't be considered for movement.
        // TODO 1: find how to fix that.
        // TODO 2: find how to make UI movement for dropping items
        // in folders and collections whilst keeping reordering movement
        // via drag-and-drop.
        {
            return (false, false);
        }

        /*var sourceItems = targetItem switch
        {
            // can receive reqsanddirsparentvms, reqsvms, envsvms
            CollectionViewModel _ => SelectedTreeItems.Where(x =>
                                                   x is CollectionFolderViewModel ||
                                                   x is HttpRequestViewModel ||
                                                   x is WebSocketConnectionViewModel ||
                                                   x is HttpRepeaterViewModel ||
                                                   x is EnvironmentViewModel),
            // can receive envsvms
            EnvironmentsGroupViewModel _ => SelectedTreeItems.Where(x => x is EnvironmentViewModel),
            // can receive reqsanddirsparentvms, reqsvms
            CollectionFolderViewModel _ => SelectedTreeItems.Where(x =>
                                                   x is CollectionFolderViewModel ||
                                                   x is HttpRequestViewModel ||
                                                   x is WebSocketConnectionViewModel ||
                                                   x is HttpRepeaterViewModel),
            // parent can receive envsvms
            EnvironmentViewModel _ => SelectedTreeItems.Where(x => x is EnvironmentViewModel),
            // parent can receive reqsanddirsparentvms, reqsvms
            HttpRequestViewModel _ => SelectedTreeItems.Where(x =>
                                                   x is CollectionFolderViewModel ||
                                                   x is HttpRequestViewModel ||
                                                   x is WebSocketConnectionViewModel ||
                                                   x is HttpRepeaterViewModel),
            // can receive wsclimsgsvms
            // parent can receive reqsanddirsparentvms, reqsvms
            WebSocketConnectionViewModel _ => SelectedTreeItems.Where(x =>
                                                   x is CollectionFolderViewModel ||
                                                   x is HttpRequestViewModel ||
                                                   x is WebSocketConnectionViewModel ||
                                                   x is HttpRepeaterViewModel ||
                                                   x is WebSocketClientMessageViewModel),
            // parent can receive wsclimsgsvms
            WebSocketClientMessageViewModel _ => SelectedTreeItems.Where(x => x is WebSocketClientMessageViewModel),
            // parent can receive reqsanddirsparentvms, reqsvms
            HttpRepeaterViewModel _ => SelectedTreeItems.Where(x =>
                                                   x is CollectionFolderViewModel ||
                                                   x is HttpRequestViewModel ||
                                                   x is WebSocketConnectionViewModel ||
                                                   x is HttpRepeaterViewModel),
            _ => [],
        };*/
        return RunDropActions(e, bExecute, sourceItem, targetItem);
    }

    private (bool Valid, bool WillSourceItemBeMovedToDifferentParent) RunDropActions(
        DragEventArgs e,
        bool bExecute,
        CollectionOrganizationItemViewModel sourceItem,
        CollectionOrganizationItemViewModel targetItem)
    {
        IList sourceParentCol;
        ICollectionOrganizationItemParentViewModel? targetParent = null;
        IList? targetParentCol = null;

        switch (sourceItem)
        {
            case HttpRequestViewModel _:
                return CheckDropActionForRequestOrFolder(e, bExecute, sourceItem, targetItem);
            case HttpRepeaterViewModel _:
                return CheckDropActionForRequestOrFolder(e, bExecute, sourceItem, targetItem);
            case WebSocketConnectionViewModel _:
                return CheckDropActionForRequestOrFolder(e, bExecute, sourceItem, targetItem);
            case CollectionFolderViewModel _:
                return CheckDropActionForRequestOrFolder(e, bExecute, sourceItem, targetItem);
            case WebSocketClientMessageViewModel _:
                sourceParentCol = ((WebSocketConnectionViewModel)sourceItem.Parent).Items;
                if (targetItem is WebSocketConnectionViewModel wsConnVm)
                {
                    targetParent = wsConnVm;
                    targetParentCol = wsConnVm.Items;
                }
                else if (targetItem.Parent is WebSocketConnectionViewModel wsConnVm2)
                {
                    targetParent = wsConnVm2;
                    targetParentCol = wsConnVm2.Items;
                }
                return RunDropAction(e, bExecute, sourceItem, targetItem, sourceParentCol, targetParentCol, targetParent);
            case EnvironmentViewModel _:
                sourceParentCol = ((EnvironmentsGroupViewModel)sourceItem.Parent).Items;
                if (targetItem is EnvironmentViewModel envVm)
                {
                    targetParent = ((EnvironmentsGroupViewModel)envVm.Parent);
                    targetParentCol = ((EnvironmentsGroupViewModel)envVm.Parent).Items;
                }
                else if (targetItem is EnvironmentsGroupViewModel envGrpVm)
                {
                    targetParent = envGrpVm;
                    targetParentCol = envGrpVm.Items;
                }
                return RunDropAction(e, bExecute, sourceItem, targetItem, sourceParentCol, targetParentCol, targetParent);
            case CollectionViewModel _:
                sourceParentCol = ((MainWindowViewModel)sourceItem.Parent).CollectionsGroupViewDataCtx.Items;
                if (targetItem is CollectionViewModel colVm)
                {
                    targetParent = (MainWindowViewModel)colVm.Parent;
                    targetParentCol = ((MainWindowViewModel)colVm.Parent).CollectionsGroupViewDataCtx.Items;
                }
                else if (targetItem is CollectionsGroupViewModel colGrpVm)
                {
                    targetParent = colGrpVm.Parent;
                    targetParentCol = colGrpVm.Items;
                }
                return RunDropAction(e, bExecute, sourceItem, targetItem, sourceParentCol, targetParentCol, targetParent);
            default:
                return (false, false);
        }
    }

    private (bool Valid, bool WillSourceItemBeMovedToDifferentParent) CheckDropActionForRequestOrFolder(DragEventArgs e, bool bExecute, CollectionOrganizationItemViewModel sourceItem, CollectionOrganizationItemViewModel targetItem)
    {
        if (targetItem.IsDescendantOf(sourceItem))
            return (false, false);

        var sourceParentCol = ((RequestsAndFoldersParentViewModel)sourceItem.Parent).Items;
        var targetParent = targetItem is not CollectionVariablesViewModel
                           && targetItem is not CollectionScopedAuthViewModel
                           && targetItem is not EnvironmentsGroupViewModel
                           && targetItem.Parent is RequestsAndFoldersParentViewModel rafpvm ? rafpvm :
                           targetItem is CollectionViewModel cvm ? cvm : null;
        var targetParentCol = targetParent?.Items;

        return RunDropAction(e, bExecute, sourceItem, targetItem, sourceParentCol, targetParentCol, targetParent);
    }

    private (bool Valid, bool WillSourceItemBeMovedToDifferentParent) RunDropAction(
        DragEventArgs e,
        bool bExecute,
        CollectionOrganizationItemViewModel sourceItem,
        object targetItem,
        IList sourceCol,
        IList? targetCol,
        ICollectionOrganizationItemParentViewModel? newParentVm)
    {
        if (targetCol is null)
            return (false, false);

        // source and target collections are different
        if (sourceCol != targetCol)
        {
            int sourceIndex = sourceCol.IndexOf(sourceItem);

            if (sourceIndex < 0)
            {
                return (false, true);
            }

            if (e.DragEffects.HasFlag(DragDropEffects.Move))
            {
                if (bExecute)
                {
                    object item = sourceCol[sourceIndex]!;
                    sourceCol.RemoveAt(sourceIndex);
                    targetCol.Add(item); // always adding to the end
                    sourceItem.Parent = newParentVm!;
                }
                return (true, true);
            }
        }
        // source and target collections are the same
        else
        {
            int sourceIndex = sourceCol.IndexOf(sourceItem);
            int targetIndex = targetCol.IndexOf(targetItem);

            if (sourceIndex < 0 || targetIndex < 0)
            {
                return (false, false);
            }

            if (e.DragEffects.HasFlag(DragDropEffects.Move))
            {
                if (bExecute)
                {
                    if (sourceIndex < targetIndex)
                    {
                        object? item = sourceCol[sourceIndex];
                        sourceCol.RemoveAt(sourceIndex);
                        sourceCol.Insert(targetIndex, item);
                    }
                    else
                    {
                        int removeIndex = sourceIndex + 1;
                        if (sourceCol.Count + 1 > removeIndex)
                        {
                            object? item = sourceCol[sourceIndex];
                            sourceCol.RemoveAt(removeIndex - 1);
                            sourceCol.Insert(targetIndex, item);
                        }
                    }
                }
                return (true, false);
            }
        }

        return (false, false);
    }
}