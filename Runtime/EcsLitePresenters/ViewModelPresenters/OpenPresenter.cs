using System;
using unityPresenting.Unity;
using ViewModel;

namespace presenting.ecslite.EcsLitePresenters.ViewModelPresenters
{
    public class OpenPresenter : AbstractPresenter<OpenPresenter, IViewModel>
    {
        public string OpenCommandKey;
        [PresenterKeyProperty(typeof(EcsPresenterData), typeof(IViewModel))] public string PresenterKey;
        [ViewKeyProperty(typeof(IViewModel))] public string ViewModelKey;

        private IViewModelEvent<NullData> NullEvent;
        private Action<NullData> _action;

        public OpenPresenter() : base()
        {
            _action = OpenPresenterMethod;
        }

        private void OpenPresenterMethod(NullData obj)
        {
            var presenter = PresenterResolver.Resolve<EcsPresenterData, IViewModel>(PresenterKey);
            var viewModel = ViewResolver.Resolve<IViewModel>(ViewModelKey);
            presenter.Initialize(new EcsPresenterData()
            {
                ModelEntity = EcsPresenterData.ModelEntity,
                ModelWorld = EcsPresenterData.ModelWorld,
            }, viewModel);
        }

        public override void Initialize(EcsPresenterData ecsPresenterData, IViewModel viewModel)
        {
            base.Initialize(ecsPresenterData, viewModel);
            NullEvent = viewModel.GetViewModelData<IViewModelEvent<NullData>>(OpenCommandKey);
            var disposable = NullEvent.Subscribe(_action);
            AddTo(disposable);
            viewModel.Subscribe(disposable);
        }
    }
}