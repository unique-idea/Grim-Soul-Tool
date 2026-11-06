using Engine.Enums;
using Engine.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Services.Interfaces
{
    public interface IAction
    {
        void InteractItem();
        void CollectItem();
        void OpenChest();
        void DoubleClickItem(Input input);
        void CraftItem(Item item);
        void PlaceAllItems();

        /// <summary>
        /// Sử dụng phím H để hồi máu (Quick Heal).
        /// Trả về true nếu thực hiện thành công, false nếu đang trong thời gian chờ (cooldown).
        /// </summary>
        bool Heal();

        /// <summary>
        /// Thời gian chờ cooldown hồi máu (ms) để tránh over-heal.
        /// </summary>
        int HealCooldownMs { get; set; }

        void Attack();
        void StartHoldingAttack();
        void StopHoldingAttack();
        void PressAttack();
        bool IsHoldingAttack { get; }
        void ChangeWeapon();
        void Crouch();
        void PressC(bool ignoreBlock = false);
        void PressX();
        void CloseChest();
        void ClickAt(int x, int y);
        void DoubleClickAt(int x, int y);
        void PressR();
        void PressO();
        void PressY();

        /// <summary>
        /// Bật chế độ tự động farm / nhặt tài nguyên bằng phím A (Auto Farm).
        /// </summary>
        void AutoFarm();

        // Advance
        void InitInventory();
        void FinishFarmingTurn();
    }
}
