using System.Reactive;
using Pororoca.Desktop.HotKeys;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using ReactiveUI;

namespace Pororoca.Desktop.ViewModels.DataGrids;

public sealed class HttpResponseCapturesDataGridViewModel : BaseDataGridWithOperationsViewModel<HttpResponseCaptureViewModel, PororocaHttpResponseValueCapture>
{
    public override SimpleClipboardArea<PororocaHttpResponseValueCapture> InnerClipboardArea =>
        HttpResponseCapturesClipboardArea.Instance;

    public ReactiveCommand<Unit, Unit> AddNewHeaderCaptureCmd { get; }
    public ReactiveCommand<Unit, Unit> AddNewBodyCaptureCmd { get; }

    public HttpResponseCapturesDataGridViewModel(List<PororocaHttpResponseValueCapture>? initialValues) : base(initialValues)
    {
        AddNewHeaderCaptureCmd = ReactiveCommand.Create(AddNewHeaderCapture);
        AddNewBodyCaptureCmd = ReactiveCommand.Create(AddNewBodyCapture);
    }

    protected override HttpResponseCaptureViewModel ToVm(PororocaHttpResponseValueCapture domainObj) =>
        new(Items, domainObj);

    protected override PororocaHttpResponseValueCapture ToDomain(HttpResponseCaptureViewModel viewModel) =>
        viewModel.ToResponseCapture();

    protected override PororocaHttpResponseValueCapture MakeCopy(PororocaHttpResponseValueCapture domainObj) =>
        domainObj.Copy();

    private void AddNewHeaderCapture() =>
        Items.Add(new(Items, new(PororocaHttpResponseValueCaptureType.Header, "var", "header_name", null)));

    private void AddNewBodyCapture() =>
        Items.Add(new(Items, new(PororocaHttpResponseValueCaptureType.Body, "var", null, "$.myProp")));
}