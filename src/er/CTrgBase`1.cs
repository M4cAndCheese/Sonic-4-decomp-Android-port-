// Decompiled with JetBrains decompiler
// Type: er.CTrgBase`1
// Assembly: Sonic4 ep I, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 093CE2FC-33E2-4332-B0FE-1EA1E44D3AE7
// Assembly location: C:\Users\zakga\Desktop\Sonic 4\Sonic4 ep I.dll

using System;
using System.Runtime.InteropServices;

namespace er
{

    public class CTrgBase<T> where T : CTrgState, new()
    {
      private const int c_touch_max = 4;
      private bool[] m_flag = new bool[2];
      private readonly T[] m_state = AppMain.New<T>(4);
      private IntPair[] m_pos = new IntPair[4];

      public virtual void Update()
      {
        if (!this.IsValid() || this.m_flag[0])
          return;
        for (int index = 0; index < AppMain._am_tp_touch.Length; ++index)
        {
          bool is_on = false;
          AppMain.AMS_TP_TOUCH_STATUS amsTpTouchStatus = AppMain._am_tp_touch[index];
          bool is_edge;
          IntPair move;
          if (AppMain.amTpIsTouchOn(index))
          {
            is_edge = AppMain.amTpIsTouchPush(index);
            if (is_edge)
              this.m_pos[index] = new IntPair((int) amsTpTouchStatus.push[0], (int) amsTpTouchStatus.push[1]);
            IntPair pos = new IntPair((int) amsTpTouchStatus.on[0], (int) amsTpTouchStatus.on[1]);
            if (!this.m_flag[1])
              is_on = this.hitTest(pos, (uint) index);
            move = pos - this.m_pos[index];
            this.m_pos[index] = pos;
          }
          else
          {
            is_edge = AppMain.amTpIsTouchPull(index);
            move = new IntPair((int) amsTpTouchStatus.pull[0], (int) amsTpTouchStatus.pull[1]) - this.m_pos[index];
            if (is_edge)
              this.m_pos[index] = new IntPair();
          }
          this.m_state[index].Push(is_on, is_edge, move);
        }
      }

      public virtual bool IsValid() => false;

      public void ResetState()
      {
        for (int index = 0; index < this.m_state.Length; ++index)
          this.m_state[index].ResetState();
      }

      public void ResetState(uint index) => this.m_state[(int) index].ResetState();

      public void AddLock() => this.getState().AddLock();

      public void AddLock(uint index) => this.m_state[(int) index].ResetState();

      public void DelLock() => this.getState().DelLock();

      public void DelLock(uint index) => this.getState(index).DelLock();

      public bool IsFrieze() => this.m_flag[0];

      public bool IsNoHit() => this.m_flag[1];

      public T GetState() => this.getState();

      public T GetState(uint index) => this.getState(index);

      public IntPair GetRepeatInterval() => this.GetState(0U).GetRepeatInterval();

      public int GetDoubleClickTime() => this.GetState(0U).GetDoubleClickTime();

      public int GetMoveThreshold() => this.GetState(0U).GetMoveThreshold();

      public void SetFrieze(bool frieze) => this.m_flag[0] = frieze;

      public void SetNoHit(bool nohit) => this.m_flag[1] = nohit;

      public void SetRepeatInterval(IntPair repeat_interval)
      {
        for (int index = 0; index < this.m_state.Length; ++index)
          this.m_state[index].SetRepeatInterval(repeat_interval);
      }

      public void SetRepeatInterval(int first, int second)
      {
        for (int index = 0; index < this.m_state.Length; ++index)
          this.m_state[index].SetRepeatInterval(first, second);
      }

      public void SetRepeatInterval()
      {
        for (int index = 0; index < this.m_state.Length; ++index)
          this.m_state[index].SetRepeatInterval();
      }

      public void SetDoubleClickTime(int wc_time)
      {
        for (int index = 0; index < this.m_state.Length; ++index)
          this.m_state[index].SetDoubleClickTime(wc_time);
      }

      public void SetDoubleClickTime()
      {
        for (int index = 0; index < this.m_state.Length; ++index)
          this.m_state[index].SetDoubleClickTime();
      }

      public void SetMoveThreshold(int move_threshold)
      {
        for (int index = 0; index < this.m_state.Length; ++index)
          this.m_state[index].SetMoveThreshold(move_threshold);
      }

      public void SetMoveThreshold()
      {
        for (int index = 0; index < this.m_state.Length; ++index)
          this.m_state[index].SetMoveThreshold();
      }

      protected virtual bool hitTest(IntPair pos, uint index) => false;

      private T getState()
      {
        for (int index = 0; index < this.m_state.Length; ++index)
        {
          if (this.m_state[index][14])
            return this.m_state[index];
        }
        for (int index = 0; index < this.m_state.Length; ++index)
        {
          if (this.m_state[index][0])
            return this.m_state[index];
        }
        return this.m_state[0];
      }

      private T getState(uint index) => this.m_state[(int) index];

      [StructLayout(LayoutKind.Sequential, Size = 1)]
      private struct BFlag
      {
        public const int Frieze = 0;
        public const int NoHit = 1;
        public const int Max = 2;
        public const int None = 3;
      }
    }
}
