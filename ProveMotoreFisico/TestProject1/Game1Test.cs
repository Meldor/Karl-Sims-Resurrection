using ProveMotoreFisico;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.Xna.Framework;

namespace TestProject1
{
    
    
    /// <summary>
    ///Classe di test per Game1Test.
    ///Creata per contenere tutti gli unit test Game1Test
    ///</summary>
    [TestClass()]
    public class Game1Test
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Ottiene o imposta il contesto dei test, che fornisce
        ///funzionalità e informazioni sull'esecuzione dei test corrente.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Attributi di test aggiuntivi
        // 
        //Durante la scrittura dei test è possibile utilizzare i seguenti attributi aggiuntivi:
        //
        //Utilizzare ClassInitialize per eseguire il codice prima di eseguire il primo test della classe
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Utilizzare ClassCleanup per eseguire il codice dopo l'esecuzione di tutti i test di una classe
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Utilizzare TestInitialize per eseguire il codice prima di eseguire ciascun test
        //
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //Utilizzare TestCleanup per eseguire il codice dopo l'esecuzione di ciascun test
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///Test per Costruttore Game1
        ///</summary>
        [TestMethod()]
        public void Game1ConstructorTest()
        {
            Game1 target = new Game1();
            Assert.Inconclusive("TODO: Implementare il codice per la verifica della destinazione");
        }

        /// <summary>
        ///Test per Draw
        ///</summary>
        [TestMethod()]
        [DeploymentItem("ProveMotoreFisico.exe")]
        public void DrawTest()
        {
            Game1_Accessor target = new Game1_Accessor(); // TODO: Eseguire l'inizializzazione a un valore appropriato
            GameTime gameTime = null; // TODO: Eseguire l'inizializzazione a un valore appropriato
            target.Draw(gameTime);
            Assert.Inconclusive("Impossibile verificare un metodo che non restituisce valori.");
        }

        /// <summary>
        ///Test per Initialize
        ///</summary>
        [TestMethod()]
        [DeploymentItem("ProveMotoreFisico.exe")]
        public void InitializeTest()
        {
            Game1_Accessor target = new Game1_Accessor(); // TODO: Eseguire l'inizializzazione a un valore appropriato
            target.Initialize();
            Assert.Inconclusive("Impossibile verificare un metodo che non restituisce valori.");
        }

        /// <summary>
        ///Test per LoadContent
        ///</summary>
        [TestMethod()]
        [DeploymentItem("ProveMotoreFisico.exe")]
        public void LoadContentTest()
        {
            Game1_Accessor target = new Game1_Accessor(); // TODO: Eseguire l'inizializzazione a un valore appropriato
            target.LoadContent();
            Assert.Inconclusive("Impossibile verificare un metodo che non restituisce valori.");
        }

        /// <summary>
        ///Test per UnloadContent
        ///</summary>
        [TestMethod()]
        [DeploymentItem("ProveMotoreFisico.exe")]
        public void UnloadContentTest()
        {
            Game1_Accessor target = new Game1_Accessor(); // TODO: Eseguire l'inizializzazione a un valore appropriato
            target.UnloadContent();
            Assert.Inconclusive("Impossibile verificare un metodo che non restituisce valori.");
        }

        /// <summary>
        ///Test per Update
        ///</summary>
        [TestMethod()]
        [DeploymentItem("ProveMotoreFisico.exe")]
        public void UpdateTest()
        {
            Game1_Accessor target = new Game1_Accessor(); // TODO: Eseguire l'inizializzazione a un valore appropriato
            GameTime gameTime = null; // TODO: Eseguire l'inizializzazione a un valore appropriato
            target.Update(gameTime);
            Assert.Inconclusive("Impossibile verificare un metodo che non restituisce valori.");
        }
    }
}
