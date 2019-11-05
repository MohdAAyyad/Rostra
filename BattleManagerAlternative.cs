using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManagerAlternative : BattleManager {
	public override void AddEnemy(int enemyIndex, int agi, int str, int crit, int speed, int currentHp, int maxHp, Enemy enemyRef, string name) {
		base.AddEnemy(enemyIndex, agi, str, crit, speed, currentHp, maxHp, enemyRef, name);
	}

	public override void BuildQueue() {
		base.BuildQueue();
	}

	public override void EndOfBattle(bool victory) {
		base.EndOfBattle(victory);
	}

	public override bool Equals(object other) {
		return base.Equals(other);
	}

	public override int GetHashCode() {
		return base.GetHashCode();
	}

	public override void LevelUp(int playerIndex) {
		base.LevelUp(playerIndex);
	}

	public override void NextOnQueue() {
		base.NextOnQueue();
	}

	public override void StartBattle() {
		base.StartBattle();
	}

	public override string ToString() {
		return base.ToString();
	}

	public override void UpdateFromExp(int playerIndex, int currentExp, int maxExp) {
		base.UpdateFromExp(playerIndex, currentExp, maxExp);
	}

	public override void UpdatePlayerStats(int playerIndex) {
		base.UpdatePlayerStats(playerIndex);
	}

	protected override void Awake() {
		base.Awake();
	}

	protected override void Start() {
		base.Start();
	}

	protected override void TellInventoryToUpdateConsumables() {
		base.TellInventoryToUpdateConsumables();
	}

	protected override void Update() {
		base.Update();
	}
}
