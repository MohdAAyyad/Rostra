using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBTLAlternative : UIBTL {
	public override void AddImageToQ(Sprite nextOnQImage) {
		base.AddImageToQ(nextOnQImage);
	}

	public override void DisableActivtyText() {
		base.DisableActivtyText();
	}

	public override void EndTurn() {
		base.EndTurn();
	}

	public override void EnemyIsDead(int enemyIndex) {
		base.EnemyIsDead(enemyIndex);
	}

	public override void ImageRecycle(int imageIndex) {
		base.ImageRecycle(imageIndex);
	}

	public override void MoveQImages() {
		base.MoveQImages();
	}

	public override void PlayerIsDead(int playerIndex) {
		base.PlayerIsDead(playerIndex);
	}

	public override void QueueIsReady() {
		base.QueueIsReady();
	}

	public override void RageOptionTextColor() {
		base.RageOptionTextColor();
	}

	public override void ShowThisPlayerUI(int playerIndex, string name, Player playerReference) {
		base.ShowThisPlayerUI(playerIndex, name, playerReference);
	}

	public override void StartShowingEndScreen(bool isVictory) {
		base.StartShowingEndScreen(isVictory);
	}

	public override string ToString() {
		return base.ToString();
	}

	public override void UpdateActivityText(string activity) {
		base.UpdateActivityText(activity);
	}

	public override void UpdateNumberOfEndTurnsNeededToEndTurn(int rowCount) {
		base.UpdateNumberOfEndTurnsNeededToEndTurn(rowCount);
	}

	public override void UpdatePlayerHPControlPanel() {
		base.UpdatePlayerHPControlPanel();
	}

	public override void UpdatePlayerMPControlPanel() {
		base.UpdatePlayerMPControlPanel();
	}

	protected override void ChoosingAllEnemies() {
		base.ChoosingAllEnemies();
	}

	protected override void ChoosingAllPlayers() {
		base.ChoosingAllPlayers();
	}

	protected override void ChoosingBasicCommand() {
		base.ChoosingBasicCommand();
	}

	protected override void ChoosingEnemy() {
		base.ChoosingEnemy();
	}

	protected override void ChoosingItemsCommand() {
		base.ChoosingItemsCommand();
	}

	protected override void ChoosingPlayer() {
		base.ChoosingPlayer();
	}

	protected override void ChoosingRowOfEnemies() {
		base.ChoosingRowOfEnemies();
	}

	protected override void ChoosingSkillsCommand() {
		base.ChoosingSkillsCommand();
	}

	protected override void EndBattleUI() {
		base.EndBattleUI();
	}

	protected override void MoveEnemyIndicatorToFirstAliveEnemy() {
		base.MoveEnemyIndicatorToFirstAliveEnemy();
	}

	protected override void Start() {
		base.Start();
	}

	protected override void Update() {
		base.Update();
	}

	protected override void UpdatePlayerStats(int playerIndex) {
		base.UpdatePlayerStats(playerIndex);
	}
}
