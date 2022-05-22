using System;
using Leopotam.EcsLite;

namespace presenting.ecslite
{
    [Serializable]
    public struct EcsPresenterData
    {
        public EcsWorld ModelWorld;
        public int ModelEntity;
    }
}