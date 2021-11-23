using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TheEngine.ECS
{
    public sealed class ComponentDataAccess<T> where T : unmanaged, IComponentData
    {
        private readonly unsafe byte* data;
        private readonly int[] sparseReverseEntityMapping;

        public unsafe ComponentDataAccess(byte* data, int[] sparseReverseEntityMapping)
        {
            this.data = data;
            this.sparseReverseEntityMapping = sparseReverseEntityMapping;
        }
        
        public unsafe ref T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                //fixed (byte* p = data)
                return ref *(T*)(data + index * sizeof(T));
                //return ref Unsafe.AsRef<T>(p + index * sizeof(T));
            }
        }

        public unsafe ref T this[Entity index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                var indexInArray = sparseReverseEntityMapping[index.Id] - 1;
                //fixed(byte* p = data)
                return ref Unsafe.AsRef<T>(data + indexInArray * sizeof(T));
            }
        }
    }
}