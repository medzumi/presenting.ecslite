using ApplicationScripts.Ecs;
using ApplicationScripts.Ecs.Utility;
using Leopotam.EcsLite;

namespace presenting.ecslite
{
    public class ViewModelUpdateSystem<TComponent> : EcsSystemBase
        where TComponent : struct
    {
        private EcsCollector _ecsCollector;
        private EcsPool<TComponent> _componentPool;
        private EcsPool<ListComponent<IUpdatable<TComponent>>> _componentBindDataPool;

        public ViewModelUpdateSystem() : base()
        {
            
        }

        public override void PreInit(EcsSystems systems)
        {
            var world = systems.GetWorld();
            _ecsCollector = world.Filter<TComponent>().Inc<ListComponent<IUpdatable<TComponent>>>()
                .EndCollector(CollectorEvent.Added | CollectorEvent.Dirt);
            _componentPool = world.GetPool<TComponent>();
            _componentBindDataPool = world.GetPool<ListComponent<IUpdatable<TComponent>>>();
        }

        public override void Run(EcsSystems systems)
        {
            foreach (var entity in _ecsCollector)
            {
                var componentData = _componentPool.Get(entity);
                foreach (var ecsPresenter in _componentBindDataPool.Get(entity).List)
                {
                    ecsPresenter.Update(componentData);
                }
            }
            _ecsCollector.Clear();
        }
    }
}