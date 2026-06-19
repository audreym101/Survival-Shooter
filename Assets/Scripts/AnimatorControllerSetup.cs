using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.Animations;
using UnityEditor;

/// <summary>
/// Creates Animator Controllers for enemies in the Assets/Assets folder.
/// This script runs in the editor to set up animation controllers.
/// </summary>
public class AnimatorControllerSetup : MonoBehaviour
{
    [MenuItem("Tools/Setup Enemy Animators")]
    public static void SetupAnimatorControllers()
    {
        string controllerPath = "Assets/Animations/EnemyAnimator.controller";
        
        // Create Animations folder if it doesn't exist
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
        {
            AssetDatabase.CreateFolder("Assets", "Animations");
        }

        // Check if controller already exists
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        
        if (controller == null)
        {
            // Create new controller
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            Debug.Log("✓ Created EnemyAnimator.controller");
        }

        // Get the root state machine
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        
        // Clear existing states (keep default)
        foreach (var childState in stateMachine.states)
        {
            if (childState.state.name != "Entry" && childState.state.name != "Exit")
            {
                stateMachine.RemoveState(childState.state);
            }
        }

        // Add parameters
        AddParameter(controller, "isWalking", AnimatorControllerParameterType.Bool);
        AddParameter(controller, "attack", AnimatorControllerParameterType.Trigger);
        AddParameter(controller, "shoot", AnimatorControllerParameterType.Trigger);
        AddParameter(controller, "die", AnimatorControllerParameterType.Trigger);

        // Create states (they can be empty - just for animation triggers)
        var idleState = stateMachine.AddState("Idle");
        var walkState = stateMachine.AddState("Walk");
        var attackState = stateMachine.AddState("Attack");
        var shootState = stateMachine.AddState("Shoot");
        var dieState = stateMachine.AddState("Die");

        // Set Idle as default
        stateMachine.defaultState = idleState;

        // Create transitions
        // Idle → Walk
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.If, 0, "isWalking");
        idleToWalk.duration = 0.1f;

        // Walk → Idle
        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isWalking");
        walkToIdle.duration = 0.1f;

        // Any state → Attack
        var anyToAttack = stateMachine.AddAnyStateTransition(attackState);
        anyToAttack.AddCondition(AnimatorConditionMode.If, 0, "attack");
        anyToAttack.duration = 0.05f;

        // Any state → Shoot
        var anyToShoot = stateMachine.AddAnyStateTransition(shootState);
        anyToShoot.AddCondition(AnimatorConditionMode.If, 0, "shoot");
        anyToShoot.duration = 0.05f;

        // Any state → Die
        var anyToDie = stateMachine.AddAnyStateTransition(dieState);
        anyToDie.AddCondition(AnimatorConditionMode.If, 0, "die");
        anyToDie.duration = 0.1f;

        // Attack → Idle (auto-return)
        var attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.duration = 0.2f;
        attackToIdle.hasExitTime = true;
        attackToIdle.exitTime = 0.9f;

        // Shoot → Idle (auto-return)
        var shootToIdle = shootState.AddTransition(idleState);
        shootToIdle.duration = 0.2f;
        shootToIdle.hasExitTime = true;
        shootToIdle.exitTime = 0.9f;

        AssetDatabase.SaveAssets();
        Debug.Log("✓ Enemy Animator Controller setup complete!");
        Debug.Log("✓ Now assign this controller to your enemy prefab Animators");
    }

    private static void AddParameter(AnimatorController controller, string name, AnimatorControllerParameterType type)
    {
        // Check if parameter already exists
        foreach (var param in controller.parameters)
        {
            if (param.name == name)
                return; // Already exists
        }

        controller.AddParameter(name, type);
    }
}

#endif
