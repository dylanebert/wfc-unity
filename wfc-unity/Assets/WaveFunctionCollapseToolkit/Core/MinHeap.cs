using System.Collections;
using System.Collections.Generic;

namespace Wave {
    public interface IHeapItem {
        public int value { get; }
    }

    public class MinHeap<T> : IEnumerable<T> where T : IHeapItem {
        T[] heap;
        int count;
        int size;

        public MinHeap(int size) {
            this.size = size;
            heap = new T[size];
        }

        public void Push(T item) {
            if(count >= size - 1) {
                Logging.Log("Heap full", LoggingLevel.Error);
                return;
            }
            count++;
            heap[count] = item;
            BubbleUp(count);
        }

        public T Pop() {
            if(count == 0) {
                Logging.Log("Heap empty", LoggingLevel.Error);
                return default(T);
            }
            T result = heap[count];
            count--;
            if(count != 0)
                BubbleDown(1);
            return result;
        }

        public void Clear() {
            count = 0;
        }

        int Parent(int index) => index >> 1;
        int Left(int index) => index << 1;
        int Right(int index) => index << 1 | 1;

        void BubbleDown(int index) {
            int left = Left(index);
            int right = Right(index);
            while(right <= count) {
                if(heap[index].value > heap[left].value) {
                    if(heap[left].value > heap[right].value) {
                        Swap(index, right);
                        index = right;
                    } else {
                        Swap(index, left);
                        index = left;
                    }
                } else {
                    if(heap[index].value > heap[right].value) {
                        Swap(index, right);
                        index = right;
                    } else {
                        index = left;
                        left = Left(index);
                        break;
                    }
                }

                left = Left(index);
                right = Right(index);
            }

            if(left <= count && heap[index].value > heap[left].value)
                Swap(index, left);
        }

        void BubbleUp(int index) {
            int parent = Parent(index);
            while(parent > 0 && heap[parent].value > heap[index].value) {
                Swap(parent, index);
                index = parent;
                parent = Parent(index);
            }
        }

        void Swap(int a, int b) {
            T temp = heap[a];
            heap[a] = heap[b];
            heap[b] = temp;
        }

        public IEnumerator<T> GetEnumerator() {
            return (IEnumerator<T>)heap.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}