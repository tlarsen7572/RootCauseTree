using System;
using System.Collections.Generic;
using System.Text;

namespace com.PorcupineSupernova.RootCauseTreeCore
{
    class CreateProblemContainer : IRootCauseCommand
    {
        private ProblemContainer _Container;
        private IRootCauseDb _Db;

        public bool Executed { get; private set; }
        public ProblemContainer Container { get { return _Container; } }

        public CreateProblemContainer(IRootCauseDb db, string initialProblemText) : this(db, initialProblemText, false) { }

        public CreateProblemContainer(IRootCauseDb db,string initialProblemText,bool executeImmediately)
        {
            _Container = new ProblemContainer(initialProblemText);
            _Db = db;
            if (executeImmediately) { Execute(); }
        }

        public void Execute()
        {
            if (Executed) { throw new CommandAlreadyExecutedException(); }
            if (!_Db.InsertTopLevel(_Container.InitialProblem)) { throw new CommandFailedDbWriteException(); }
            Executed = true;
        }

        public void Undo()
        {
            if (!Executed) { throw new CommandNotExecutedException(); }
            if (!_Db.RemoveTopLevel(_Container.InitialProblem)) { throw new CommandFailedDbWriteException(); }
            Executed = false;
        }
    }
}
