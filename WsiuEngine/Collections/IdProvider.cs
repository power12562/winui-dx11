using System;
using System.Collections;
using System.Collections.Generic;
using WsiuEngine.Core.System;

namespace WsiuEngine.Collections
{
    [SerializableClass]
    public class IdProvider : ReflectionObject.ISerializationCallback
    {
        [SerializeField]
        [HideInInspector]
        private UInt64 _idCounter = 0;

        [SerializeField]
        [HideInInspector]
        private UInt64 _maxId = UInt64.MaxValue;
        public UInt64 MaxID 
        { 
            get => _maxId;  
            set => _maxId = value;
        }

        private Queue<UInt64> _reusableIds = new();
        [SerializeField]
        [HideInInspector]
        private UInt64[]? _reusableIdsBuffer;

        [SerializeField]
        [HideInInspector]
        private HashSet<UInt64> _activeIds = [];

        public IEnumerator GetActiveIds()
        {
            return _activeIds.GetEnumerator();
        }

        public UInt64 Generate()
        {
            UInt64 id;
            if(_reusableIds.Count > 0)
                id = _reusableIds.Dequeue();
            else
            {
                if (_idCounter == _maxId)
                    throw new InvalidOperationException("The IdProvider has reached its maximum ID capacity and cannot generate a new ID.");

                id = _idCounter++;
            }
                
            _activeIds.Add(id);
            return id;
        }

        public void Release(UInt64 id)
        {
            if (_activeIds.Remove(id))
                _reusableIds.Enqueue(id);
            //else // TODO: 이미 파괴되었거나 존재하지 않는 ID에 대한 경고 로그 필요
        }

        public IdProvider Current => throw new NotImplementedException();

        public void OnBeforeSerialize()
        {
            _reusableIdsBuffer = _reusableIds.ToArray();
        }

        public void OnAfterDeserialize()
        {
            if (_reusableIdsBuffer != null)
            {
                _reusableIds = new(_reusableIdsBuffer);
                _reusableIdsBuffer = null;
            }
        }
    }
}
