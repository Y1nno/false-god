import random

# --- CONFIGURATION ---
NUM_FLOORS = 40
NUM_TRIALS = 1000

def calculate_enemy_stats(base_hp, base_atk, base_def, level):
    l_factor = level - 1
    hp = base_hp * (1 + l_factor * 0.20) # 20% instead of 25%?
    atk = base_atk * (1 + l_factor * 0.15) # 15% instead of 18%?
    df = base_def * (1 + l_factor * 0.10)
    return hp, atk, df

def simulate_run(religion_type="None"):
    level = 1
    xp = 0
    xp_threshold = 13
    max_hp = 100
    hp = max_hp
    
    str_stat = 10
    dex_stat = 10
    
    weapon_atk = 5 # Starting sword
    armor_def = 2 # Starting armor
    
    faith_level = 1
    fervor_stacks = 0
    potions = 2 # Start with some
    
    for floor in range(1, NUM_FLOORS + 1):
        # Floor starts
        encounters = random.randint(5, 10)
        for e in range(encounters):
            e_type = random.random()
            
            if e_type < 0.15: # Altar/Bonus
                if faith_level < 4: faith_level += 1
            elif e_type < 0.25: # Rest (Full HP)
                hp = max_hp
            elif e_type < 0.8: # Combat
                enemy_hp, enemy_atk, enemy_def = calculate_enemy_stats(12, 3, 1, floor)
                
                # Religion Buffs
                p_atk = (str_stat + dex_stat)/5 + weapon_atk
                if religion_type == "Whisper" and faith_level >= 4:
                    fervor_stacks = min(5, fervor_stacks + 1)
                    p_atk *= (1 + fervor_stacks * 0.02)
                
                p_def = armor_def
                
                while enemy_hp > 0 and hp > 0:
                    enemy_hp -= (p_atk - enemy_def)
                    if enemy_hp <= 0:
                        total_xp = 5 + floor * 0.5
                        xp += total_xp
                        while xp >= xp_threshold:
                            xp -= xp_threshold
                            level += 1
                            str_stat += 2
                            dex_stat += 2
                            max_hp = 100 + str_stat * 3 # buffed HP scaling
                            xp_threshold = xp_threshold + 10 + (level * level * 2.5) # slower xp threshold
                        break
                    
                    hp -= max(1, enemy_atk - p_def)
                    
                    # Panic heal
                    if hp < max_hp * 0.3 and potions > 0:
                        hp = min(max_hp, hp + max_hp * 0.5)
                        potions -= 1
                
                if hp <= 0: return floor
                
                # Drop chance for potion
                if random.random() < 0.15: potions += 1
            
            # Shop / Drop
            if random.random() < 0.1: # Gear find
                weapon_atk = max(weapon_atk, 5 + floor * 1.5 * (1.5 if religion_type == "Forge" else 1))
                armor_def = max(armor_def, 2 + floor * 0.8)
                
        fervor_stacks = 0
    return 41

def run_batch(religion_name):
    res = [simulate_run(religion_name) for _ in range(NUM_TRIALS)]
    survived = [r for r in res if r == 41]
    deaths = [r for r in res if r != 41]
    avg_death = sum(deaths)/len(deaths) if deaths else 40
    print(f"[{religion_name}] Win Rate: {len(survived)/NUM_TRIALS:.1%} | Avg Death Floor: {avg_death:.1f}")

print("Simulation with Potions and Optimized Gear/Stat Scaling:")
run_batch("None")
run_batch("Whisper")
run_batch("Forge")
