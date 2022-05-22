using System;
using System.Collections.Generic;
using UnityEngine;
using unityPresenting.Core;
using unityPresenting.Unity;
using Utilities.Unity.SerializeReferencing;
using ViewModel;

namespace presenting.ecslite.ViewModelPresenters
{
    public sealed class AggregatePresenter : AbstractPresenter<AggregatePresenter, IViewModel>
    {
        [SerializeField] [PresenterKeyProperty(typeof(EcsPresenterData), typeof(IViewModel))]
        private List<string> _presenterKeys = new List<string>();

        private readonly List<IPresenter<EcsPresenterData, IViewModel>> _presenters =
            new List<IPresenter<EcsPresenterData, IViewModel>>();

        public override void Initialize(EcsPresenterData ecsPresenterData, IViewModel view)
        {
            base.Initialize(ecsPresenterData, view);
            foreach (var ecsPresenterKey in _presenterKeys)
            {
                var ecsPresenter = PresenterResolver.Resolve<EcsPresenterData, IViewModel>(ecsPresenterKey);
                _presenters.Add(ecsPresenter);
                if (ecsPresenter is IPresenter<EcsPresenterData, IViewModel> presenter)
                {
                    presenter.Initialize(ecsPresenterData, view);
                }
            }
        }

        public AggregatePresenter() : base()
        {
        }

        public AggregatePresenter(List<IPresenter<EcsPresenterData, IViewModel>> presenters) : this()
        {
            _presenters.AddRange(presenters);
        }

        protected override void DisposeHandler()
        {
            base.DisposeHandler();
            foreach (var ecsPresenter in _presenters)
            {
                if (ecsPresenter is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _presenters.Clear();
        }

        protected override AggregatePresenter CloneHandler()
        {
            var clone = base.CloneHandler();
            clone._presenterKeys.Clear();
            clone._presenterKeys.AddRange(_presenterKeys);

            return clone;
        }
    }
}