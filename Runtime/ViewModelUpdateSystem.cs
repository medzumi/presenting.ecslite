using System.Collections.Generic;
using ApplicationScripts.Ecs;
using ApplicationScripts.Ecs.Utility;
using Leopotam.EcsLite;

namespace presenting.ecslite
{
    public class ViewModelUpdateSystem<TComponent> : EcsSystemBase, IEcsRunSystem
        where TComponent : struct
    {
        private EcsCollector _ecsCollector;
        private EcsCollector _emptyCollector;
        private EcsPool<TComponent> _componentPool;
        private EcsPool<ListComponent<IUpdatable<TComponent>>> _componentBindDataPool;
        private List<int> _buffer = new List<int>();

        public override void PreInit(EcsSystems systems)
        {
            var world = systems.GetWorld();
            _ecsCollector = world.Filter<TComponent>().Inc<ListComponent<IUpdatable<TComponent>>>()
                .EndCollector(CollectorEvent.Added | CollectorEvent.Dirt);
            _emptyCollector = world.Filter<ListComponent<IUpdatable<TComponent>>>().Exc<TComponent>().EndCollector(CollectorEvent.Added | CollectorEvent.Dirt);
            _componentPool = world.GetPool<TComponent>();
            _componentBindDataPool = world.GetPool<ListComponent<IUpdatable<TComponent>>>();
        }

        public void Run(EcsSystems systems)
        {
            _emptyCollector.GetEntitiesWithPreClear(_buffer);
            foreach (var entity in _buffer)
            {
                var list = _componentBindDataPool.Get(entity).List;
                for (int i = 0; i < list.Count; i++)
                {
                    list[i].Update(null);
                }
            }
            
            _ecsCollector.GetEntitiesWithPreClear(_buffer);
            foreach (var entity in _buffer)
            {
                var componentData = _componentPool.Get(entity);
                var list = _componentBindDataPool.Get(entity).List;
                for (int i =  0; i < list.Count; i++)
                {
                    var ecsPresenter = list[i];
                    ecsPresenter.Update(componentData);
                }
            }
        }
    }
}