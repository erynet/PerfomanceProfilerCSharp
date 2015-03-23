using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// https://github.com/erynet/SymmetricMinMaxHeapCSharp/
namespace Direct3DHookLib.Hook.Common
{
    class SMMHeap
    {
        private Int32 _Size;
        private Int32 _ArraySize;

        private Int32 _InitialSize;

        private List<Int64> L;


        public SMMHeap(Int32 InitialSize = 4)
        {
            this._Size = 2;

            if (InitialSize < 4)
                this._ArraySize = 4;
            else
                this._ArraySize = InitialSize;

            this.L = new List<Int64>(this._ArraySize);
            this.L.AddRange(Enumerable.Repeat<Int64>(Int64.MinValue, this._ArraySize));

            this._InitialSize = InitialSize;
        }

        ///////////////////
        // Inner Methods //
        ///////////////////

        private static Int32 uLnode(Int32 index)
        {
            Int32 temp = (index / 4) * 2;
            if (temp == 0)
                return 0;
            else
                return temp;
        }
        private static Int32 uRnode(Int32 index)
        {
            Int32 temp = (index / 4) * 2;
            if (temp == 0)
                return 0;
            else
                return temp + 1;
        }
        //private static Int32 llLnode(Int32 index)
        //{
        //    return index * 2;
        //}
        //private static Int32 llRnode(Int32 index)
        //{
        //    return (index * 2) + 2;
        //}
        //private static Int32 lrLnode(Int32 index)
        //{
        //    return (index * 2) - 1;
        //}
        //private static Int32 lrRnode(Int32 index)
        //{
        //    return (index * 2) + 1;
        //}
        private void Swap(Int32 indexA, Int32 indexB)
        {
            Int64 temp = this.L[indexA];
            this.L[indexA] = this.L[indexB];
            this.L[indexB] = temp;
        }
        private void Resize(Int32 size)
        {
            Int32 CurrentSize = this.L.Count;
            if (size < CurrentSize)
                this.L.RemoveRange(size, CurrentSize - size);
            else if (size > CurrentSize)
            {
                if (size > this.L.Capacity)
                    this.L.Capacity = size;
                this.L.AddRange(Enumerable.Repeat<Int64>(Int64.MinValue, size - CurrentSize));
            }
        }
        private void Expand()
        {
            Int32 _TargetArraySize = this._ArraySize * 2;
            this.Resize(_TargetArraySize);
            this._ArraySize = _TargetArraySize;
        }


        public void Insert(Int64 value)
        {
            this.L[this._Size] = value;
            Int32 _CurrentIndex = this._Size;
            this._Size++;
            if ((_CurrentIndex % 2) == 1)
            {
                if (this.L[_CurrentIndex - 1] > this.L[_CurrentIndex])
                {
                    this.Swap(_CurrentIndex - 1, _CurrentIndex);
                    _CurrentIndex--;
                }
            }

            Int32 _lnode = 0, _rnode = 0;
            while (true)
            {
                _lnode = SMMHeap.uLnode(_CurrentIndex);
                _rnode = SMMHeap.uRnode(_CurrentIndex);
                if ((_lnode != 0) && (this.L[_CurrentIndex] < this.L[_lnode]))
                {
                    this.Swap(_CurrentIndex, _lnode);
                    _CurrentIndex = _lnode;
                }
                else if ((_rnode != 0) && (this.L[_CurrentIndex] > this.L[_rnode]))
                {
                    this.Swap(_CurrentIndex, _rnode);
                    _CurrentIndex = _rnode;
                }
                else
                {
                    break;
                }
            }

            if (this._Size >= this._ArraySize)
                this.Expand();
        }

        private Int64 TakeMin()
        {
            if (this._Size > 2)
            {
                return this.L[2];
            }
            else
                return 0;
        }
        public Int64 Min
        {
            get { return TakeMin(); }
        }

        private Int64 TakeMax()
        {
            if (this._Size > 3)
            {
                return this.L[3];
            }
            else if (this._Size == 3)
            {
                return this.L[2];
            }
            else
                return 0;
        }
        public Int64 Max
        {
            get { return TakeMax(); }
        }

        public void DeleteMin()
        {
            this._Size--;
            this.L[2] = this.L[this._Size];
            int _CurrentIndex = 2;

            Int32 _lnode = 0, _rnode = 0;
            while (true)
            {
                _lnode = _CurrentIndex * 2;
                _rnode = (_CurrentIndex * 2) + 2;

                if (_rnode < this._Size)
                {
                    if (this.L[_lnode] < this.L[_rnode])
                    {
                        if (this.L[_lnode] < this.L[_CurrentIndex])
                        {
                            if (this.L[_lnode] > this.L[_lnode + 1])
                            {
                                this.Swap(_lnode + 1, _CurrentIndex);
                                _CurrentIndex = (_lnode + 1);
                            }
                            else
                            {
                                this.Swap(_lnode, _CurrentIndex);
                                _CurrentIndex = _lnode;
                            }
                        }
                        else
                            break;

                    }
                    else if (this.L[_lnode] >= this.L[_rnode])
                    {
                        if (this.L[_rnode] < this.L[_CurrentIndex])
                        {

                            if ((_rnode + 1) < this._Size)
                            {
                                if (this.L[_rnode] > this.L[_rnode + 1])
                                {
                                    this.Swap(_rnode + 1, _CurrentIndex);
                                    _CurrentIndex = (_rnode + 1);
                                    continue;
                                }
                            }

                            this.Swap(_rnode, _CurrentIndex);
                            _CurrentIndex = _rnode;
                        }
                        else
                            break;
                    }
                }
                else if (_lnode < this._Size)
                {
                    if (this.L[_lnode] < this.L[_CurrentIndex])
                    {
                        if ((_lnode + 1) < this._Size)
                        {
                            if (this.L[_lnode] > this.L[_lnode + 1])
                            {
                                this.Swap(_lnode + 1, _CurrentIndex);
                                _CurrentIndex = (_lnode + 1);
                                continue;
                            }
                        }

                        this.Swap(_lnode, _CurrentIndex);
                        _CurrentIndex = _lnode;
                    }
                    else
                        break;
                }
                else
                {
                    if ((_CurrentIndex + 1) < this._Size)
                    {
                        if (this.L[_CurrentIndex] > this.L[_CurrentIndex + 1])
                            this.Swap(_CurrentIndex, _CurrentIndex + 1);
                    }
                    break;
                }
            }
        }

        public void DeleteMax()
        {
            this._Size--;
            this.L[3] = this.L[this._Size];
            int _CurrentIndex = 3;

            Int32 _lnode = 0, _rnode = 0;
            while (true)
            {
                _lnode = (_CurrentIndex * 2) - 1;
                _rnode = (_CurrentIndex * 2) + 1;

                if (_rnode < this._Size)
                {
                    if (this.L[_lnode] > this.L[_rnode])
                    {
                        if (this.L[_lnode] > this.L[_CurrentIndex])
                        {
                            if (this.L[_lnode] < this.L[_lnode + 1])
                            {
                                this.Swap(_lnode + 1, _CurrentIndex);
                                _CurrentIndex = (_lnode + 1);
                            }
                            else
                            {
                                this.Swap(_lnode, _CurrentIndex);
                                _CurrentIndex = _lnode;
                            }
                        }
                        else
                            break;

                    }
                    else if (this.L[_lnode] <= this.L[_rnode])
                    {
                        if (this.L[_rnode] > this.L[_CurrentIndex])
                        {

                            if ((_rnode + 1) < this._Size)
                            {
                                if (this.L[_rnode] < this.L[_rnode + 1])
                                {
                                    this.Swap(_rnode + 1, _CurrentIndex);
                                    _CurrentIndex = (_rnode + 1);
                                    continue;
                                }
                            }

                            this.Swap(_rnode, _CurrentIndex);
                            _CurrentIndex = _rnode;
                        }
                        else
                            break;
                    }
                }
                else if (_lnode < this._Size)
                {
                    if (this.L[_lnode] > this.L[_CurrentIndex])
                    {
                        if ((_lnode + 1) < this._Size)
                        {
                            if (this.L[_lnode] < this.L[_lnode + 1])
                            {
                                this.Swap(_lnode + 1, _CurrentIndex);
                                _CurrentIndex = (_lnode + 1);
                                continue;
                            }
                        }

                        this.Swap(_lnode, _CurrentIndex);
                        _CurrentIndex = _lnode;
                    }
                    else
                        break;
                }
                else
                {
                    if ((_CurrentIndex + 1) < this._Size)
                    {
                        if (this.L[_CurrentIndex] < this.L[_CurrentIndex + 1])
                            this.Swap(_CurrentIndex, _CurrentIndex + 1);
                    }
                    break;
                }
            }
        }

        public void Clear()
        {
            this._Size = 2;
            this._ArraySize = this._InitialSize;
            this.Resize(this._ArraySize);
        }

        private Int32 TakeSize()
        {
            return this._Size - 2;
        }
        public Int32 Size
        {
            get { return TakeSize(); }
        }

        public List<Int64> Array
        {
            get { return this.L; }
        }

    }
    
}
