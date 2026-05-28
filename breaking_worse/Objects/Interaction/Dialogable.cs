using System;
using breaking_worse.Input.Enums;
using breaking_worse.Objects.Animations;
using breaking_worse.Objects.Collisions;
using breaking_worse.Objects.Player;
using breaking_worse.Screens.ScreenTypes.InGameScreens;

namespace breaking_worse.Objects.Interaction;

public class Dialogable(GameManager gameManager, Func<PlayerCharacterId, DialogueScreen> createDialogueScreen, int radius, bool isCookingStation = false) : IComponent
{
    private DialogueScreen _dialogueScreenWalt;
    private DialogueScreen _dialogueScreenJesse;
    private int _countOpen = 0; // to check if a dialogue is open (for animation of Trader)

    public void update(Collider collider, PlayerCharacterId playerCharacter = PlayerCharacterId.Walt)
    {
        if (!isCookingStation)
        {
            if (playerCharacter == PlayerCharacterId.Jesse)
            {
                checkForPlayersInRadius(collider, gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse);
            }
            else
            {
                checkForPlayersInRadius(collider, gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt);
            }
            
            if (gameManager.ScreenManager.InGameScreen.PlayerState.Health == 0 ||
                gameManager.ScreenManager.InGameScreen.PlayerState.IsCaught)
            {
                if (_dialogueScreenWalt != null)
                {
                    gameManager.ScreenManager.removeFromStack(_dialogueScreenWalt);
                    _dialogueScreenWalt = null;
                    gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsInteracting = false;
                    _countOpen--;
                }
                else if (_dialogueScreenJesse != null)
                {
                    gameManager.ScreenManager.removeFromStack(_dialogueScreenJesse);
                    _dialogueScreenJesse = null;
                    gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsInteracting = false;
                    _countOpen--;
                }
            }
        }
        else checkForPlayersInRadiusCooking(collider);
    }

    private void checkForPlayersInRadiusCooking(Collider collider)
    {
        // check if either walt or jesse presses interaction key
        if (!gameManager.UserActionHandler.isUserAction(PlayerCharacterId.Walt, UserAction.Interact, PressType.PressAndRelease) 
            && !gameManager.UserActionHandler.isUserAction(PlayerCharacterId.Jesse, UserAction.Interact, PressType.PressAndRelease))
            return;
        
        // check if both walt and jesse are in radius
        var neighbors = gameManager.ScreenManager.InGameScreen.CollisionManager.getNeighborsInRadius(collider, radius);
        if (!neighbors.Contains(gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<Collider>())
            || !neighbors.Contains(gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<Collider>()))
            return;
        
        // if button pressed and already interacting, close dialogue
        if (gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsInteracting && gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsInteracting)
        {
            quit();
        }
        // if button pressed and not already interacting, open dialogue
        else
        {
            gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsInteracting = true;
            gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsInteracting = true;
            _dialogueScreenWalt = createDialogueScreen(PlayerCharacterId.Walt);
            gameManager.ScreenManager.addScreen(_dialogueScreenWalt);
        }
    }

    private void checkForPlayersInRadius(Collider collider, PlayerCharacter playerCharacter)
    {
        var playerCharacterId = playerCharacter.PlayerCharacterId;
        // check if interaction key is pressed
        if (!gameManager.UserActionHandler.isUserAction(playerCharacterId, UserAction.Interact, PressType.PressAndRelease)) return;
        
        // check if player is in radius
        if (!collider.isInRadius(playerCharacter.getComponent<Collider>(), radius)) return;
        
        // if button pressed and already interacting, close dialogue
        if (playerCharacter.IsInteracting)
        {
            playerCharacter.IsInteracting = false;
            if (playerCharacterId == PlayerCharacterId.Walt)
            {
                gameManager.ScreenManager.removeFromStack(_dialogueScreenWalt);
                _dialogueScreenWalt = null;
                _countOpen--;
            }
            else
            {
                gameManager.ScreenManager.removeFromStack(_dialogueScreenJesse);
                _dialogueScreenJesse = null;
                _countOpen--;
            }

        }
        // if button pressed and not already interacting, open dialogue
        else
        {
            playerCharacter.IsInteracting = true;
            if (playerCharacterId == PlayerCharacterId.Walt)
            {
                _dialogueScreenWalt = createDialogueScreen(playerCharacterId);
                gameManager.ScreenManager.addScreen(_dialogueScreenWalt);
                gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.getComponent<Animatable>().startAnimation("idle", true);
                gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.getComponent<Animatable>().startAnimation("idle", true);
                _countOpen++;
            }
            else
            {
                _dialogueScreenJesse = createDialogueScreen(playerCharacterId);
                gameManager.ScreenManager.addScreen(_dialogueScreenJesse);
                _countOpen++;
            } 
            playerCharacter.checkForPoliceInRadius();
        }
    }

    public void quit()
    {
        gameManager.ScreenManager.InGameScreen.GameObjectManager.Walt.IsInteracting = false;
        gameManager.ScreenManager.InGameScreen.GameObjectManager.Jesse.IsInteracting = false;
        gameManager.ScreenManager.removeFromStack(_dialogueScreenWalt);
        _dialogueScreenWalt = null;
    }
    
    public bool IsClosed => _countOpen == 0;
}
