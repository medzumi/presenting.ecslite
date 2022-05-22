using System;
using ApplicationScripts.Ecs;
using ApplicationScripts.Ecs.Utility;
using Game.CoreLogic;
using Leopotam.EcsLite;
using presenting.Unity.Default;
using unityPresenting.Core;
using Utilities;
using Utilities.Pooling;
using ViewModel;

namespace presenting.ecslite
{
    [Serializable]
    public abstract class AbstractPresenter<TPresenter, TView> : PoolableObject<TPresenter>, IPresenter<EcsPresenterData, TView>, IInject<IPresenterResolver>, IInject<IViewResolver>
        where TPresenter : AbstractPresenter<TPresenter, TView>, new()
        where TView : IDisposeHandler
    {
        public IViewResolver ViewResolver;
        public IPresenterResolver PresenterResolver;
         
        private EcsPresenterData _ecsPresenterData;
        private TView _view;

        protected EcsPresenterData EcsPresenterData
        {
            get => _ecsPresenterData;
            private set => _ecsPresenterData = value;
        }

        protected TView View => _view;
        
        protected EcsPool<DisposableListComponent> DisposeComponentPool { get; private set; }

        public virtual void Initialize(EcsPresenterData ecsPresenterData, TView view)
        {
            _view = view;
            EcsPresenterData = ecsPresenterData;
            DisposeComponentPool = ecsPresenterData.ModelWorld.GetPool<DisposableListComponent>();
            DisposeComponentPool.EnsureGet(ecsPresenterData.ModelEntity).List.Add(this);
            view.Subscribe(this);
        }

        public IPresenter<EcsPresenterData, TView> Clone()
        {
            return CloneHandler();
        }

        protected virtual TPresenter CloneHandler()
        {
            var clone = AbstractPresenter<TPresenter, TView>.Create();
            clone.PresenterResolver = PresenterResolver;
            clone.ViewResolver = ViewResolver;
            return clone;
        }

        public Type GetModelType()
        {
            return typeof(EcsPresenterData);
        }

        public Type GetViewType()
        {
            return typeof(TView);
        }

        public void Initialize(object model, object view)
        {
            Initialize((EcsPresenterData)model, (TView)view);
        }
        void IInject<IPresenterResolver>.Inject(IPresenterResolver injectable)
        {
            PresenterResolver = injectable;
            PresenterResolverInjected(injectable);
        }

        void IInject<IViewResolver>.Inject(IViewResolver injectable)
        {
            ViewResolver = injectable;
            ViewResolverInjected(injectable);
        }

        protected virtual void PresenterResolverInjected(IPresenterResolver presenterResolver)
        {
            
        }

        protected virtual void ViewResolverInjected(IViewResolver viewResolver)
        {
            
        }
    }
    
    [Serializable]
    public abstract class AbstractPresenter<TPresenter, TView, TData> : AbstractPresenter<TPresenter, TView>, IUpdatable<TData>
        where TPresenter : AbstractPresenter<TPresenter, TView, TData>, new()
        where TData : struct
        where TView : IDisposeHandler
    {
        public string HasComponentKey;
        
        protected EcsPool<ListComponent<IUpdatable<TData>>> _updatablePool;

        public override void Initialize(EcsPresenterData ecsPresenterData, TView view)
        {
            base.Initialize(ecsPresenterData, view);
            _updatablePool = ecsPresenterData.ModelWorld.GetPool<ListComponent<IUpdatable<TData>>>();
            var component = _updatablePool.EnsureGet(ecsPresenterData.ModelEntity);
            component.List.Add(this);
            _updatablePool.Get(ecsPresenterData.ModelEntity) = component;
        }

        public virtual void Update(TData? data)
        {
            if (data.HasValue)
            {
                Update(data.Value);
            }
        }

        protected virtual void Update(TData data)
        {
            
        }

        protected override void DisposeHandler()
        {
            base.DisposeHandler();
            if (_updatablePool.Has(EcsPresenterData.ModelEntity))
            {
                var data = _updatablePool.Get(EcsPresenterData.ModelEntity).List;
                data.Remove(this);
                if (data.Count == 0)
                {
                    _updatablePool.Del(EcsPresenterData.ModelEntity);
                }
            }
        }
    }
}