using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;
using Biggy.Data.Postgres;
using Biggy.Core;

namespace Tests.Postgres {
  [TestFixture()]
  [Category("PG Relational Store")]
  public class PgRelationalStoreWithStringKey {

    PgDbCore _db;

    [SetUp]
    public void init() {
      _db = new PgDbCore("biggy_test");
      DropCreateTestTables();
    }

    void DropCreateTestTables() {
      string BuildingTableSql = ""
        + "CREATE TABLE \"Building\" ( \"BIN\" text NOT NULL, \"Identifier\" text, \"PropertyId\" text, CONSTRAINT pk_building_bin PRIMARY KEY (\"BIN\"))";
      _db.TryDropTable("Building");
      if (!_db.TableExists("Building")) {
        _db.TransactDDL(BuildingTableSql);
      }
    }

    [Test()]
    public void Relational_Store_Inserts_record_with_string_id() {
      var theBin = "OR13-22";
      var BuildingStore = new PgRelationalStore<Building>(_db);
      var newBuilding = new Building { BIN = "OR13-22", Identifier = "Building A", PropertyId = 1 };
      BuildingStore.Add(newBuilding);

      var foundBuilding = BuildingStore.TryLoadData().FirstOrDefault();
      Assert.IsTrue(foundBuilding != null && foundBuilding.BIN == theBin);
    }

    [Test()]
    public void Relational_Store_Inserts_range_of_records_with_string_id() {
      var BuildingStore = new PgRelationalStore<Building>(_db);
      var myBatch = new List<Building>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        var newBuilding = new Building { BIN = "OR13-" + i, Identifier = "Building " + i, PropertyId = i };
        myBatch.Add(newBuilding);
      }
      BuildingStore.Add(myBatch);
      var buildings = BuildingStore.TryLoadData();
      Assert.IsTrue(buildings.Count == qtyToAdd);
    }

    [Test()]
    public void Relational_Store_Updates_record_with_string_id() {
      var BuildingStore = new PgRelationalStore<Building>(_db);
      var newBuilding = new Building { BIN = "OR13-55", Identifier = "Building C", PropertyId = 1 };
      BuildingStore.Add(newBuilding);

      // Now go fetch the record again and update:
      string newIdentifier = "Updated Building C";
      var foundBuilding = BuildingStore.TryLoadData().FirstOrDefault();
      foundBuilding.Identifier = newIdentifier;
      BuildingStore.Update(foundBuilding);

      foundBuilding = BuildingStore.TryLoadData().FirstOrDefault(b => b.BIN == "OR13-55");
      Assert.IsTrue(foundBuilding != null && foundBuilding.Identifier == newIdentifier);
    }

    [Test()]
    public void Relational_Store_Updates_range_of_records_with_string_id() {
      var BuildingStore = new PgRelationalStore<Building>(_db);
      var myBatch = new List<Building>();
      int qtyToAdd = 10;
      for (int i = 1; i <= qtyToAdd; i++) {
        var newBuilding = new Building { BIN = "OR13-" + i, Identifier = "Building " + i, PropertyId = i };
        myBatch.Add(newBuilding);
      }
      BuildingStore.Add(myBatch);

      // Re-load, and update:
      var buildings = BuildingStore.TryLoadData();
      for (int i = 0; i < qtyToAdd; i++) {
        buildings.ElementAt(i).Identifier = "Updated Building " + i;
      }
      BuildingStore.Update(buildings);

      // Reload, and check updated names:
      buildings = BuildingStore.TryLoadData().Where(c => c.Identifier.Contains("Updated")).ToList();
      Assert.IsTrue(buildings.Count == qtyToAdd);
    }

    [Test()]
    public void Relational_Store_Deletes_record_with_string_id() {
      var BuildingStore = new PgRelationalStore<Building>(_db);
      var newBuilding = new Building { BIN = "OR300-01", Identifier = "Building D", PropertyId = 1 };
      BuildingStore.Add(newBuilding);

      // Load from back-end:
      var buildings = BuildingStore.TryLoadData();
      int qtyAdded = buildings.Count;

      // Delete:
      var foundBuilding = buildings.FirstOrDefault();
      BuildingStore.Delete(foundBuilding);

      int remaining = BuildingStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == 1 && remaining == 0);
    }

    [Test()]
    public void Relational_Store_Deletes_range_of_records_with_string_id() {
      var BuildingStore = new PgRelationalStore<Building>(_db);
      var myBatch = new List<Building>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        var newBuilding = new Building { BIN = "OR400-" + i, Identifier = "Building " + i, PropertyId = i };
        myBatch.Add(newBuilding);
      }
      BuildingStore.Add(myBatch);

      // Re-load from back-end:
      var buildings = BuildingStore.TryLoadData();
      int qtyAdded = buildings.Count;

      // Select 5 for deletion:
      int qtyToDelete = 5;
      var deleteThese = new List<Building>();
      for (int i = 0; i < qtyToDelete; i++) {
        deleteThese.Add(buildings.ElementAt(i));
      }

      // Delete:
      BuildingStore.Delete(deleteThese);
      int remaining = BuildingStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == qtyAdded - qtyToDelete);
    }

    [Test()]
    public void Relational_Store_Deletes_all_records_with_string_id() {
      var BuildingStore = new PgRelationalStore<Building>(_db);
      var myBatch = new List<Building>();
      int qtyToAdd = 10;
      for (int i = 0; i < qtyToAdd; i++) {
        var newBuilding = new Building { BIN = "OR500-" + i, Identifier = "Building " + i, PropertyId = i };
        myBatch.Add(newBuilding);
      }
      BuildingStore.Add(myBatch);

      // Re-load from back-end:
      var buildings = BuildingStore.TryLoadData();
      int qtyAdded = buildings.Count;

      // Delete:
      BuildingStore.DeleteAll();
      int remaining = BuildingStore.TryLoadData().Count;
      Assert.IsTrue(qtyAdded == qtyToAdd && remaining == 0);
    }
  }
}

