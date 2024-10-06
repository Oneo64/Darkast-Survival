using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Database : Object
{
	// Item name
	// Model name, bullet name, animation to use, ammunition, sound close, sound far
	// Maximum bullets, automatic, min damage, max damage, bullet speed
	// Fire rate, spread, bullets per shot

	public static Dictionary<string, IItem> items = new Dictionary<string, IItem>() {
		{"makarov", new Gun(
			"Makarov", "bullet",
			new string[] {"makarov", "HoldPistol", "pistol_magazine"},
			8, false, 40, 60, 315,
			0.1f, 0.25f, 0.73f
		)},
		{"glock_17", new Gun(
			"Glock 17", "bullet",
			new string[] {"glock", "HoldPistol", "pistol_magazine"},
			17, false, 40, 60, 375,
			0.05f, 0.15f, 0.62f
		)},
		{"colt_navy", new Gun(
			"Colt 1851 Navy Revolver", "bullet",
			new string[] {"coltnavy", "HoldPistol", ".38_rimfire_box"},
			6, false, 50, 70, 250,
			0.4f, 0.5f, 1.2f
		)},

		{"ak_47", new Gun(
			"AK 47", "bullet",
			new string[] {"ak", "HoldRifle", "assault_rifle_magazine"},
			30, true, 50, 70, 715,
			0.1f, 0.25f, 3.47f
		)},
		{"handmade_rifle", new Gun(
			"Handmade Rifle", "bullet",
			new string[] {"handmaderifle", "HoldRifle", "assault_rifle_magazine"},
			20, true, 30, 40, 400,
			0.15f, 1f, 2
		)},
		{"mossberg_500", new Gun(
			"Mossberg 500", "bullet",
			new string[] {"mossberg500", "HoldRifle", "12_guage_box"},
			8, false, 18, 20, 250,
			0.5f, 2, 3.2f, 9
		)},
		{"cz_455", new Gun(
			"CZ 455", "bullet",
			new string[] {"cz455", "HoldRifle", ".22_lr_magazine"},
			5, false, 140, 160, 770,
			1f, 0.05f, 5f
		)},
		{"p90", new Gun(
			"P90", "bullet",
			new string[] {"p90", "HoldRifle", "submachine_magazine"},
			50, true, 30, 40, 400,
			0.065f, 0.2f, 2.6f
		)},
		{"ppsh_41", new Gun(
			"PPSH 41", "bullet",
			new string[] {"ppsh41", "HoldRifle", "drum_magazine"},
			71, true, 25, 35, 490,
			0.048f, 0.3f, 3.63f
		)},
		{"handmade_fire_pistol", new Gun(
			"Handmade Fire Pistol", "bullet",
			new string[] {"flamethrower", "HoldPistol", "propane_magazine"},
			5, false, 0, 0, 0,
			2, 0, 0.5f
		)},
		{"mac_10", new Gun(
			"MAC 10", "bullet",
			new string[] {"mac10", "HoldPistol", "submachine_magazine"},
			45, true, 30, 40, 360,
			0.05f, 0.3f, 2.84f
		)},
		{"energy_rifle", new Gun(
			"Energy Rifle", "bolt",
			new string[] {"energyrifle", "HoldRifle", "battery"},
			200, true, 15, 25, 200,
			0.1f, 0.5f, 5f
		)},

		{"flashlight", new Tool("Flashlight", "flashlight", 0.1f, false)},

		{"pistol_magazine", new Tool("Pistol Magazine", "magazine", 0.2f)},
		{"submachine_magazine", new Tool("Submachine Magazine", "magazine", 0.2f)},
		{"carbine_magazine", new Tool("Carbine Magazine", "magazine", 0.2f)},

		{"assault_rifle_magazine", new Tool("Assault Rifle Magazine", "magazine", 0.3f)},
		{"drum_magazine", new Tool("Drum Magazine", "magazine", 0.4f)},
		{"12_guage_box", new Tool("12 Guage Box", "magazine", 0.3f)},
		{".22_lr_magazine", new Tool(".22 Long Rifle Magazine", "magazine", 0.3f)},
		{".38_rimfire_box", new Tool(".38 Rimfire Box", "magazine", 0.2f)},
		{"propane_magazine", new Tool("Pistol Magazine (Propane Gas)", "magazine", 0.1f)},
		{"battery", new Tool("Battery", "battery", 0.05f)},
		{"wood", new Tool("Wood Plank", "wood", 0.2f)},
		{"metal", new Tool("Metal Scrap", "metal", 0.3f)},
		{"spring", new Tool("Spring", "spring", 0.1f)},
		{"gunpowder", new Tool("Gunpowder", "gunpowder", 0.05f)},
		{"string", new Tool("String", "string", 0.05f)},
		{"rope", new Tool("Rope", "rope", 0.1f)},
		{"paper", new Tool("Sheet of Paper", "paper", 0.05f)},
		{"plastic", new Tool("Bit of Plastic", "plastic", 0.05f)},

		{"flare", new Throwable("Flare", "flare", 0.1f)},
		{"grenade", new Throwable("Grenade", "grenade", 0.4f)},
		{"dynamite", new Throwable("Dynamite Bundle", "dynamite", 1f)},

		{"canned_soup", new Food("Canned Soup", "soup", 10, 1f)},
		{"canned_tuna", new Food("Canned Tuna", "tuna", 15, 1f)},
	};

	public static Dictionary<string, Dictionary<string, int>> crafting = new Dictionary<string, Dictionary<string, int>>() {
		{"handmade_rifle", new Dictionary<string, int>() {{"wood", 20}, {"metal", 5}, {"spring", 2}}},
		{"handmade_fire_pistol", new Dictionary<string, int>() {{"wood", 10}, {"metal", 4}}},
		{"grenade", new Dictionary<string, int>() {{"gunpowder", 6}, {"metal", 4}, {"string", 1}}},
		{"flare", new Dictionary<string, int>() {{"paper", 4}, {"plastic", 2}, {"rope", 2}, {"gunpowder", 1}, {"string", 1}}},
		{"flashlight", new Dictionary<string, int>() {{"plastic", 3}, {"metal", 3}, {"battery", 1}}},
		{"dynamite", new Dictionary<string, int>() {{"paper", 10}, {"gunpowder", 15}, {"string", 10}}},
		{".38_rimfire_box", new Dictionary<string, int>() {{"paper", 2}, {"gunpowder", 3}, {"metal", 6}}},
		{"12_guage_box", new Dictionary<string, int>() {{"plastic", 2}, {"gunpowder", 3}, {"metal", 5}}},
	};

	public static Dictionary<string, int> richochetProbs = new Dictionary<string, int>() {
		{"Metal", 4},
		{"Concrete", 8},
		{"Tiles", 8},
		{"Ice", 12},
		{"Grass", 16}
	};

	public static Dictionary<string, string> landEffects = new Dictionary<string, string>() {
		{"Metal", "Metal"},
		{"Grass", "Dirt"},
		{"Dirt", "Dirt"},
		{"Bark", "Wood"},
		{"Wood", "Wood"}
	};

	public static Limb[] GetLimbs(Transform armature, Vector3 force, string specialHitName = "") {
		return GetLimbs(armature, force, specialHitName, Vector3.zero);
	}

	public static Limb[] GetLimbs(Transform armature, Vector3 force, string specialHitName, Vector3 specialHitPos) {
		List<Limb> l = new List<Limb>() {};

		foreach (Transform t in armature.GetComponentsInChildren<Transform>()) {
			if (specialHitName != "") {
				l.Add(new Limb(t.transform.name, t.transform.localPosition, t.transform.localEulerAngles, t.name == specialHitName ? force : Vector3.zero, specialHitPos));
			} else {
				l.Add(new Limb(t.transform.name, t.transform.localPosition, t.transform.localEulerAngles, t.name == "Hip" ? force : Vector3.zero, Vector3.zero));
			}
		}

		return l.ToArray();
	}
}

[System.Serializable]
public class Item {
	public string id;
	public int amount;

	public int externalData;

	public Item() {}

	public Item(string n, int a) {
		id = n;
		amount = a;
	}

	public string GetName() {
		return Database.items[id].name;
	}

	public IItem GetData() {
		return Database.items[id];
	}
}

public interface IItem {
	public string name { get; set; }
	public bool canStack { get; set; }
	public int amount { get; set; }
	public float weight { get; set; }
}

public struct Food : IItem {
	public string name { get; set; }
	public bool canStack { get; set; }
	public int amount { get; set; }
	public float weight { get; set; }

	public int heal;
	public string model;

	public Food(string n, string m, int h, float w) {
		name = n;
		model = m;

		heal = h;

		canStack = true;
		amount = 1;

		weight = w;
	}
}

public struct Junk : IItem {
	public string name { get; set; }
	public bool canStack { get; set; }
	public int amount { get; set; }
	public float weight { get; set; }

	public Junk(string n, float w) {
		name = n;

		canStack = true;
		amount = 1;

		weight = w;
	}
}

public struct Tool : IItem {
	public string name { get; set; }
	public bool canStack { get; set; }
	public int amount { get; set; }
	public float weight { get; set; }

	public string model;

	public Tool(string n, string m, float w, bool stack = true) {
		name = n;
		model = m;

		canStack = stack;
		amount = 1;

		weight = w;
	}
}

public struct Throwable : IItem {
	public string name { get; set; }
	public bool canStack { get; set; }
	public int amount { get; set; }
	public float weight { get; set; }

	public string model;

	public Throwable(string n, string m, float w, bool stack = true) {
		name = n;
		model = m;

		canStack = stack;
		amount = 1;

		weight = w;
	}
}

public struct Gun : IItem {
	public string name { get; set; }
	public bool canStack { get; set; }
	public int amount { get; set; }
	public float weight { get; set; }

	public string bullet;

	public string[] model;
	public int maxAmmunition;
	public int ammunition;
	public bool isAutomatic;

	public int minDamage;
	public int maxDamage;
	public int muzzleVelocity;

	public float fireRate;
	public float spread;
	public int shots;

	public Gun(string n, string b, string[] m, int ma, bool auto, int minDmg, int maxDmg, int vel, float fr, float s, float w, int sh = 1) {
		name = n;
		bullet = b;
		model = m;
		maxAmmunition = ma;
		ammunition = 0;
		isAutomatic = auto;

		minDamage = minDmg;
		maxDamage = maxDmg;
		muzzleVelocity = vel;

		fireRate = fr;
		spread = s;
		shots = sh;

		canStack = false;
		amount = 1;

		weight = w;
	}
}

public struct Limb {
	public string name;
	public Vector3 position;
	public Vector3 rotation;
	public Vector3 force;
	public Vector3 forcePos;

	public Limb(string n, Vector3 p, Vector3 r, Vector3 f, Vector3 fp) {
		name = n;
		position = p;
		rotation = r;
		force = f;
		forcePos = fp;
	}
}

[System.Serializable]
public struct DeathParameters {
	public Vector3 force;
	public string hitName;
	public Vector3 hitPos;

	public DeathParameters(Vector3 f, string h, Vector3 hp) {
		force = f;
		hitName = h;
		hitPos = hp;
	}
}

public enum Perk {
	NoPerk,
	Athlete,
	Engineer,
	ExplosionGuy,
	Monkey,
	Survivior
}