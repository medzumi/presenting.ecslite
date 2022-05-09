using System;
using System.Collections.Generic;
using UnityEngine;
using unityPresenting.Core;
using Utilities.Unity.SerializeReferencing;
using ViewModel;

namespace presenting.ecslite.EcsLitePresenters.ViewModelPresenters
{
    public sealed class AggregatePresenter : AbstractPresenter<AggregatePresenter, IViewModel>
    {
        [SerializeReference] [SerializeTypes(typeof(IPresenter<EcsPresenterData, IViewModel>))]
        private List<IPresenter> _presenters = new List<IPresenter>();

        public override void Initialize(EcsPresenterData ecsPresenterData, IViewModel view)
        {
            base.Initialize(ecsPresenterData, view);
            foreach (var ecsPresenter in _presenters)
            {
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
            foreach (var ecsPresenter in _presenters)
            {
                if (ecsPresenter is IPresenter<EcsPresenterData, IViewModel> presenter)
                {
                    clone._presenters.Add(presenter.Clone());    
                }
            }

            return clone;
        }
    }
}