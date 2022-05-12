using System;
using System.Collections.Generic;
using ecslite.extensions;
using unityPresenting.Core;
using unityPresenting.Unity;
using Utilities.Pooling;
using ViewModel;

namespace presenting.ecslite.ViewModelPresenters
{
    public sealed class EntityListPresenter<TListComponent> : EntityListPresenter<EntityListPresenter<TListComponent>, TListComponent> 
        where TListComponent : struct, IListComponent<int>
    {
    }
    
    public class EntityListPresenter<TPresenter, TListComponent> : AbstractPresenter<TPresenter, IViewModel, TListComponent> 
        where TListComponent : struct, IListComponent<int>
        where TPresenter : EntityListPresenter<TPresenter, TListComponent>, new()
    {
        [PresenterKeyProperty(typeof(EcsPresenterData), typeof(IViewModel))] public string ListElementPresenterKey;
        [PresenterKeyProperty(typeof(EcsPresenterData), typeof(IViewModel))] public string RootToListElementPresenterKey;
        [ViewKeyProperty(typeof(IViewModel))] public string ListElementViewModelKey;
        public string ListPropertyKey;

        private CollectionData _collectionData;
        private readonly Func<int, IViewModel> _action;
        private IPresenter<EcsPresenterData, IViewModel> _elementExamplePresenter;
        private IPresenter<EcsPresenterData, IViewModel> _rootElementExamplePresenter;

        private readonly
            Dictionary<IViewModel, (IPresenter<EcsPresenterData, IViewModel>, EcsPresenterData, int)>
            _dictionary = new Dictionary<IViewModel, (IPresenter<EcsPresenterData, IViewModel>, EcsPresenterData, int)>();

        private TListComponent _currentListComponent;
        
        public EntityListPresenter() : base()
        {
            _action = FillAction;
        }

        public override void Initialize(EcsPresenterData ecsPresenterData, IViewModel viewModel)
        {
            base.Initialize(ecsPresenterData, viewModel);
            _collectionData = viewModel.GetViewModelData<CollectionData>(ListPropertyKey);
            _elementExamplePresenter = PresenterResolver.Resolve<EcsPresenterData, IViewModel>(ListElementPresenterKey);
            _rootElementExamplePresenter = PresenterResolver.Resolve<EcsPresenterData, IViewModel>(RootToListElementPresenterKey);
        }

        private readonly List<(IViewModel, (IPresenter<EcsPresenterData, IViewModel>, EcsPresenterData, int))> _buffer = new List<(IViewModel, (IPresenter<EcsPresenterData, IViewModel>, EcsPresenterData, int))>();

        protected override void Update(TListComponent data)
        {
            base.Update(data);
            _currentListComponent = data;
            _collectionData.Fill(data.GetList(), _action);
            var list = data.GetList();

            _buffer.Clear();
            foreach (var keyValuePair in _dictionary)
            {
                var viewModel = keyValuePair.Key;
                var index = keyValuePair.Value.Item3;
                var presenterData = keyValuePair.Value.Item2;
                var presenter = keyValuePair.Value.Item1;
                if (index >= 0 && index < list.Count)
                {
                    if (presenterData.ModelEntity != list[index])
                    {
                        presenter.Dispose();
                        presenter = _elementExamplePresenter.Clone();
                        presenterData.ModelEntity = list[index];
                        presenter.Initialize(presenterData, viewModel);
                        _buffer.Add((viewModel, (presenter, presenterData, index)));
                    }
                }
            }
            foreach (var valueTuple in _buffer)
            {
                _dictionary[valueTuple.Item1] = valueTuple.Item2;
            }
        }

        protected override TPresenter CloneHandler()
        {
            var clone =  base.CloneHandler();
            clone.ListElementPresenterKey = this.ListElementPresenterKey;
            clone.ListPropertyKey = this.ListPropertyKey;
            clone.RootToListElementPresenterKey = this.RootToListElementPresenterKey;
            clone.ListElementViewModelKey = this.ListElementViewModelKey;
            return clone;
        }

        protected override void DisposeHandler()
        {
            base.DisposeHandler();
            _elementExamplePresenter.Dispose();
            ListElementPresenterKey = string.Empty;
            ListPropertyKey = string.Empty;
        }

        [Obsolete("Temporary method. Better don't use")]
        protected virtual IViewModel ResolveElementViewModel(int arg)
        {
            return ViewResolver.Resolve<IViewModel>(ListElementViewModelKey);
        }

        private IViewModel FillAction(int arg1)
        {
            var entity = _currentListComponent.GetList()[arg1];
            var viewModel = ResolveElementViewModel(entity);
            var presenter = _elementExamplePresenter
                .Clone();
            var presenterEcsData = new EcsPresenterData()
            {
                ModelWorld = EcsPresenterData.ModelWorld,
                ModelEntity = entity,
            };
            presenter.Initialize(presenterEcsData, viewModel);

            _rootElementExamplePresenter
                .Clone()
                .Initialize(new EcsPresenterData()
                {
                    ModelWorld = EcsPresenterData.ModelWorld,
                    ModelEntity = EcsPresenterData.ModelEntity,
                }, viewModel);
            var disposer = Disposer.Create();
            _dictionary.Add(viewModel, (presenter, presenterEcsData, arg1));
            disposer.dictionary = _dictionary;
            disposer.viewModel = viewModel;
            viewModel.Subscribe(disposer);
            return viewModel;
        }
        
        private class Disposer : PoolableObject<Disposer>
        {
            public Dictionary<IViewModel, (IPresenter<EcsPresenterData, IViewModel>, EcsPresenterData, int)> dictionary;
            public IViewModel viewModel;

            protected override void DisposeHandler()
            {
                base.DisposeHandler();
                dictionary.Remove(viewModel);
                dictionary = null;
                viewModel = null;
            }
        }
    }
}