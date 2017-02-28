# Arborist

Arborist is a tool to facilitate the creation of root cause trees.  Traditionally, someone performing a root cause analysis would be limited to choosing between paper and pencil or flowchart software.  Both options tend to get in the way and slow down the brainstorming process.  What is needed is a tool which can keep up with a person's thoughts and make the process of manipulating the trees quick and seamless.  Arborist seeks to meet that objective by minimizing the number of clicks needed to manipulate tree elements while automating the layout and drawing of the tree itself.  Users can now focus on developing and maturing their ideas rather than the minutiae of manually drawing charts.

## Getting Started

Clone the project to your local machine and open RootCauseTree.sln in Visual Studio 2015 or above.
Install the following NuGet packages to the solution:

* GraphSharp v1.1.0 or above
* IdGen v2.0.1 or above
* System.Data.SQLite.Core v1.0.104 or above

## Architecture

The tree data is contained in node objects.  Node objects are not meant to be manipulated directly; rather, they are manipulated through the use of commands.  Commands perform the necessary operations to ensure consistency in the node objects while providing for undo and redo operations.  Most of the business logic is contained in the command objects.  Commands also call the necessary database methods to persist the node structure.

The database layer is separated from the commands and nodes by an interface and is self-contained to persist data to an SQLite database file.  The database layer implements a read/write file lock the entire time the file is open; concurrent users are not allowed nor supported.

The UI is built with WPF and roughly incorporates the View and ViewModel elements of MVVM.  There are likely many improvements to be found here, but in its current state the UI is functional and meets our requirements.

## Tests

The RootCauseTreeTests project contains the tests for the model objects.  The tests are divided into the following files:

* CommandTests: Tests the commands which are used to interact with the nodes.
* SqliteDbTests: Tests the database layer

There are no tests available for the View or ViewModel layers.

## Authors

* Thomas Larsen, Porcupine Supernova LLC - *Initial work*

## License

This project is licensed under the MIT License - see the [License.txt](License.txt) file for details

## Acknowledgments

* The developers of SQLite for their mature and portable database, as well as the idea of using a portable database as an application file format.
* The developers of IdGen for creating an algorithm to generate unique integer identifiers in the application, without having to talk to the database first. Using IdGen was much easier than messing with UUIDs.
* The developers of GraphSharp for their robust and format-able graph layout algorithms and controls.
* [Jessica Gawinski](https://www.jessicagawinski.com/) for the beautiful application icon.  This is the second project we have enlisted her graphic design skills and the second time she exceeded our expectations.