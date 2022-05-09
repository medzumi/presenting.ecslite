using System;
using ApplicationScripts.Ecs;
using ApplicationScripts.Ecs.Utility;
using Game.CoreLogic;
using Leopotam.EcsLite;
using unityPresenting.Core;
using Utilities.Pooling;

namespace presenting.ecslite.EcsLitePresenters.ViewModelPresenters
{
    [Serializable]
    public abstract class AbstractPresenter<TPresenter, TView> : PoolableObject<TPresenter>, IPresenter<EcsPresenterData, TView>
        where TPresenter : AbstractPresenter<TPresenter, TView>, new()
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

        public void SetPresenterResolver(IPresenterResolver presenterResolver)
        {
            PresenterResolver = presenterResolver;
        }

        public void SetViewModelResolver(IViewResolver viewResolver)
        {
            ViewResolver = viewResolver;
        }

        public virtual void Initialize(EcsPresenterData ecsPresenterData, TView view)
        {
            _view = view;
            EcsPresenterData = ecsPresenterData;
            DisposeComponentPool = ecsPresenterData.ModelWorld.GetPool<DisposableListComponent>();
            DisposeComponentPool.EnsureGet(ecsPresenterData.ModelEntity).List.Add(this);
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
            throw new NotImplementedException();
        }
    }
    
    [Serializable]
    public abstract class AbstractPresenter<TPresenter, TView, TData> : AbstractPresenter<TPresenter, TView>, IUpdatable<TData>
        where TPresenter : AbstractPresenter<TPresenter, TView, TData>, new()
        where TData : struct
    {
        public string HasComponentKey;
        
        protected EcsPool<ListComponent<IUpdatable<TData>>> _updatablePool;

        public override void Initialize(EcsPresenterData ecsPresenterData, TView view)
        {
            base.Initialize(ecsPresenterData, view);
            _updatablePool = ecsPresenterData.ModelWorld.GetPool<ListComponent<IUpdatable<TData>>>();
            _updatablePool.EnsureGet(ecsPresenterData.ModelEntity).List.Add(this);
        }

        public void Update(TData? data)
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
            var data = _updatablePool.Get(EcsPresenterData.ModelEntity).List;
            data.Remove(this);
            if (data.Count == 0)
            {
                _updatablePool.Del(EcsPresenterData.ModelEntity);
            }
        }
    }
}