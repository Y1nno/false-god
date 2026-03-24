import random

# --- GLOBALS & BALANCING ---
NUM_FLOORS = 40
NUM_TRIALS = 1000

# Player Base
BASE_STR, BASE_DEX, BASE_INT, BASE_SPD = 10, 10, 10, 10
START_HP, START_MP = 100, 50

# Scaling
MONSTER_HP_SCALE = 0.25
MONSTER_ATK_SCALE = 0.18
MONSTER_DEF_SCALE = 0.12

class Equipment:
    def __init__(self, name, rarity, atk=0, df=0, sp_atk=0, sp_df=0, str_b=0, dex_b=0, int_b=0):
        self.name = name
        self.rarity = rarity # 0=Common, 1=Uncommon, 2=Rare, 3=Legendary
        self.ph_atk = atk
        self.ph_df = df
        self.sp_atk = sp_atk
        self.sp_df = sp_df
        self.str_b = str_b
        self.dex_b = dex_b
        self.int_b = int_b
        self.max_durability = 100
        self.current_durability = 100
    
    def get_mult(self):
        return 0.5 if (self.current_durability / self.max_durability) <= 0.3 else 1.0
    
    def degrade(self, amt=2):
        self.current_durability = max(0, self.current_durability - amt)

class Player:
    def __init__(self, religion="None"):
        self.level = 1
        self.xp = 0
        self.xp_next = 13
        self.str = BASE_STR
        self.dex = BASE_DEX
        self.int = BASE_INT
        self.spd = BASE_SPD
        
        self.religion = religion
        self.faith = 1
        self.quest_item = False
        
        self.weapon = Equipment("Rusty Sword", 0, atk=5)
        self.armor = Equipment("Tattered Rags", 0, df=2)
        
        self.hp = 100
        self.mp = 50
        self.potions = 2
        
        self.has_divine_shield = False
        self.fervor_stacks = 0
        
    def get_max_hp(self):
        return 100 + self.str * 2
    
    def get_max_mp(self):
        # Pale Moon L4 bonus
        bonus = 0
        if self.religion == "Moon" and self.faith >= 4:
            bonus = 5 * 20 # Approx average depth bonus
        return 50 + self.int * 1.5 + bonus

    def update_stats(self):
        self.hp = min(self.hp, self.get_max_hp())
        self.mp = min(self.mp, self.get_max_mp())

    def level_up(self):
        while self.xp >= self.xp_next:
            self.xp -= self.xp_next
            self.level += 1
            # Simple AI: Physical build for Whisper/Forge/Veil, Mage for Moon
            if self.religion == "Moon":
                self.int += 3
                self.dex += 1
                self.str += 1
            else:
                self.str += 3
                self.dex += 2
            self.xp_next = self.xp_next + 10 + (self.level**2 * 3)

def simulate_run(rel_name):
    p = Player(rel_name)
    enemies_killed = 0
    
    for floor in range(1, NUM_FLOORS + 1):
        # Standard floor flow: ~8 encounters
        encounters = random.randint(6, 10)
        p.has_divine_shield = (p.religion == "Dawnbearer" and p.faith >= 4)
        
        for e_idx in range(encounters):
            e_type = random.random()
            
            # 1. Traps (15%)
            if e_type < 0.15:
                # Floor scaling threshold
                threshold = 8 + (floor * 2) 
                stat = random.choice(["str", "dex", "int"])
                val = getattr(p, stat) + random.randint(1, 20) # Dice roll
                
                success = val >= threshold
                if not success and p.religion == "Serpent" and p.faith >= 4:
                    val = getattr(p, stat) + random.randint(1, 20) # Reroll
                    success = val >= threshold
                
                if not success:
                    dmg = 10 + (floor * 2)
                    p.hp -= dmg
                else:
                    p.xp += 5
            
            # 2. Altar (Religion)
            elif e_type < 0.22:
                if p.faith < 4: p.faith += 1
                
            # 3. Rest / Merchant
            elif e_type < 0.35:
                p.hp = min(p.get_max_hp(), p.hp + p.get_max_hp()*0.4)
                if random.random() < 0.2: # Found a potion
                    p.potions += 1
                # Gear Upgrade logic
                if random.random() < 0.3:
                    # Forge-Tongue boost
                    boost = 1.5 if (p.religion == "Forge" and p.faith >= 4) else 1.0
                    p.weapon.ph_atk = max(p.weapon.ph_atk, 5 + floor * 2 * boost)
                    p.armor.ph_df = max(p.armor.ph_df, 2 + floor * 1 * boost)
                    if p.religion == "Forge" and p.faith >= 1: 
                        p.weapon.current_durability = 100 # Free repairs
            
            # 4. Combat (65%)
            else:
                # Enemy scaling
                is_boss = (floor % 10 == 0 and e_idx == encounters - 1)
                base_hp, base_atk, base_def = (50, 15, 5) if is_boss else (15, 4, 1.5)
                e_hp, e_atk, e_def = base_hp*(1+floor*0.2), base_atk*(1+floor*0.15), base_def*(1+floor*0.1)
                
                turns = 0
                while e_hp > 0 and p.hp > 0:
                    turns += 1
                    # Player turn
                    m = p.weapon.get_mult()
                    p_atk = (p.weapon.ph_atk * m) + (p.str + p.dex)/4
                    
                    # turn 1 bonuses
                    if turns == 1:
                        if p.religion == "Veil" and p.faith >= 3: p_atk *= 1.4 # Crit
                        if p.religion == "Dawnbearer" and p.faith >= 3: p_atk *= 1.15
                    
                    # Whisper Fervor
                    if p.religion == "Whisper" and p.faith >= 4 and turns <= 4:
                        p_atk *= (1 + p.fervor_stacks * 0.02)
                    
                    dmg = max(1, p_atk - e_def)
                    e_hp -= dmg
                    
                    if e_hp <= 0:
                        enemies_killed += 1
                        p.xp += (10 if is_boss else 5)
                        if p.religion == "Whisper" and turns <= 4:
                            p.fervor_stacks = min(5, p.fervor_stacks + 1)
                        if p.religion == "Moon" and p.faith >= 1:
                            p.mp = min(p.get_max_mp(), p.mp + p.get_max_mp()*0.1)
                        break
                    
                    # Enemy turn
                    if p.has_divine_shield:
                        p.has_divine_shield = False
                        continue # Blocked
                    
                    p_def = p.armor.ph_df * p.armor.get_mult()
                    # Dodge check
                    dodge_chance = min(65, (p.dex * 0.2 + (30 if p.religion == "Veil" and turns == 1 else 0)))
                    if random.random()*100 < dodge_chance:
                        continue # Avoided
                    
                    dmg_p = max(1, e_atk - p_def)
                    p.hp -= dmg_p
                    
                    # Durability loss
                    if p.religion != "Forge" or p.faith < 4:
                        p.weapon.degrade(1)
                        p.armor.degrade(1)
                    
                    if p.hp < p.get_max_hp()*0.3 and p.potions > 0:
                        p.hp = min(p.get_max_hp(), p.hp + p.get_max_hp()*0.4)
                        p.potions -= 1
                
                if p.hp <= 0: return floor, enemies_killed
        
        p.level_up()
        
    return 41, enemies_killed

def run_set(name):
    res = [simulate_run(name) for _ in range(NUM_TRIALS)]
    wins = [r for r in res if r[0] == 41]
    avg_death = sum(r[0] for r in res if r[0] < 41) / (NUM_TRIALS - len(wins)) if len(wins) < NUM_TRIALS else 40
    print(f"[{name:12}] Win: {len(wins)/NUM_TRIALS:5.1%} | Avg Death Floor: {avg_death:4.1f}")

run_set("None")
run_set("Dawnbearer")
run_set("Moon")
run_set("Veil")
run_set("Serpent")
run_set("Whisper")
run_set("Forge")
