using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase;

namespace OpenDental.UI.Voice;

///<summary>A list of all voice commands used in the program.</summary>
public class VoiceCommandList {
	private static List<VoiceCommand> _commands=new List<VoiceCommand> {
		#region Global
		new VoiceCommand {
			Commands=
			[
				"start listening"
			],
			ActionToPerform=VoiceCommandAction.StartListening,
			ListAreas= [VoiceCommandArea.Global],
			Response="Listening"
		},
		new VoiceCommand {
			Commands=
			[
				"stop listening"
			],
			ActionToPerform=VoiceCommandAction.StopListening,
			ListAreas= [VoiceCommandArea.Global],
			Response="No longer listening"
		},
		new VoiceCommand {
			Commands=
			[
				"give feedback",
				"start giving feedback",
				"turn feedback on"
			],
			ActionToPerform=VoiceCommandAction.GiveFeedback,
			ListAreas= [VoiceCommandArea.Global],
			Response="Giving feedback"
		},
		new VoiceCommand {
			Commands=
			[
				"stop giving feedback",
				"turn feedback off"
			],
			ActionToPerform=VoiceCommandAction.StopGivingFeedback,
			ListAreas= [VoiceCommandArea.Global],
			Response="No longer giving feedback"
		},
		new VoiceCommand {
			Commands= [],
			ActionToPerform=VoiceCommandAction.DidntGetThat,
			ListAreas= [VoiceCommandArea.Global, VoiceCommandArea.VoiceMsgBox],
			Response="I didn't get that"
		},
		#endregion Global
		#region PerioChart
		new VoiceCommand {
			Commands=
			[
				"add perio exam",
				"new perio exam"
			],
			ActionToPerform=VoiceCommandAction.CreatePerioExam,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Adding perio exam"
		},
		new VoiceCommand {
			Commands= ["zero"],
			ActionToPerform=VoiceCommandAction.Zero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one"],
			ActionToPerform=VoiceCommandAction.One,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two"],
			ActionToPerform=VoiceCommandAction.Two,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three"],
			ActionToPerform=VoiceCommandAction.Three,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four"],
			ActionToPerform=VoiceCommandAction.Four,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five"],
			ActionToPerform=VoiceCommandAction.Five,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six"],
			ActionToPerform=VoiceCommandAction.Six,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven"],
			ActionToPerform=VoiceCommandAction.Seven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight"],
			ActionToPerform=VoiceCommandAction.Eight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine"],
			ActionToPerform=VoiceCommandAction.Nine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["ten"],
			ActionToPerform=VoiceCommandAction.Ten,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eleven"],
			ActionToPerform=VoiceCommandAction.Eleven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["twelve"],
			ActionToPerform=VoiceCommandAction.Twelve,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["thirteen"],
			ActionToPerform=VoiceCommandAction.Thirteen,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["fourteen"],
			ActionToPerform=VoiceCommandAction.Fourteen,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["fifteen"],
			ActionToPerform=VoiceCommandAction.Fifteen,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["sixteen"],
			ActionToPerform=VoiceCommandAction.Sixteen,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seventeen"],
			ActionToPerform=VoiceCommandAction.Seventeen,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eightteen"],
			ActionToPerform=VoiceCommandAction.Eighteen,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nineteen"],
			ActionToPerform=VoiceCommandAction.Nineteen,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		#region  Hard-Coded Triplets and Doubles
		new VoiceCommand {
			Commands= ["three two three"],
			ActionToPerform=VoiceCommandAction.ThreeTwoThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four three four"],
			ActionToPerform=VoiceCommandAction.FourThreeFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three three three"],
			ActionToPerform=VoiceCommandAction.ThreeThreeThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two two two"],
			ActionToPerform=VoiceCommandAction.TwoTwoTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four four four"],
			ActionToPerform=VoiceCommandAction.FourFourFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two one two"],
			ActionToPerform=VoiceCommandAction.TwoOneTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four three three"],
			ActionToPerform=VoiceCommandAction.FourThreeThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three three four"],
			ActionToPerform=VoiceCommandAction.ThreeThreeFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two two three"],
			ActionToPerform=VoiceCommandAction.TwoTwoThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three two two"],
			ActionToPerform=VoiceCommandAction.ThreeTwoTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five four five"],
			ActionToPerform=VoiceCommandAction.FiveFourFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five three five"],
			ActionToPerform=VoiceCommandAction.FiveThreeFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three three five"],
			ActionToPerform=VoiceCommandAction.ThreeThreeFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five three three"],
			ActionToPerform=VoiceCommandAction.FiveThreeThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four four five"],
			ActionToPerform=VoiceCommandAction.FourFourFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five four four"],
			ActionToPerform=VoiceCommandAction.FiveFourFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five five five"],
			ActionToPerform=VoiceCommandAction.FiveFiveFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three four three"],
			ActionToPerform=VoiceCommandAction.ThreeFourThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four three five"],
			ActionToPerform=VoiceCommandAction.FourThreeFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five three four"],
			ActionToPerform=VoiceCommandAction.FiveThreeFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero zero zero"],
			ActionToPerform=VoiceCommandAction.ZeroZeroZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero zero one"],
			ActionToPerform=VoiceCommandAction.ZeroZeroOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero zero two"],
			ActionToPerform=VoiceCommandAction.ZeroZeroTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero zero three"],
			ActionToPerform=VoiceCommandAction.ZeroZeroThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero zero four"],
			ActionToPerform=VoiceCommandAction.ZeroZeroFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero zero five"],
			ActionToPerform=VoiceCommandAction.ZeroZeroFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero zero six"],
			ActionToPerform=VoiceCommandAction.ZeroZeroSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero zero seven"],
			ActionToPerform=VoiceCommandAction.ZeroZeroSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero zero eight"],
			ActionToPerform=VoiceCommandAction.ZeroZeroEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero zero nine"],
			ActionToPerform=VoiceCommandAction.ZeroZeroNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero one zero"],
			ActionToPerform=VoiceCommandAction.ZeroOneZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero one one"],
			ActionToPerform=VoiceCommandAction.ZeroOneOne,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="zero one one"//Otherwise it was pronouncing 1 as 'ohney'
		},
		new VoiceCommand {
			Commands= ["zero one two"],
			ActionToPerform=VoiceCommandAction.ZeroOneTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero one three"],
			ActionToPerform=VoiceCommandAction.ZeroOneThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero one four"],
			ActionToPerform=VoiceCommandAction.ZeroOneFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero one five"],
			ActionToPerform=VoiceCommandAction.ZeroOneFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero one six"],
			ActionToPerform=VoiceCommandAction.ZeroOneSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero one seven"],
			ActionToPerform=VoiceCommandAction.ZeroOneSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero one eight"],
			ActionToPerform=VoiceCommandAction.ZeroOneEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero one nine"],
			ActionToPerform=VoiceCommandAction.ZeroOneNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero two zero"],
			ActionToPerform=VoiceCommandAction.ZeroTwoZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero two one"],
			ActionToPerform=VoiceCommandAction.ZeroTwoOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero two two"],
			ActionToPerform=VoiceCommandAction.ZeroTwoTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero two three"],
			ActionToPerform=VoiceCommandAction.ZeroTwoThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero two four"],
			ActionToPerform=VoiceCommandAction.ZeroTwoFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero two five"],
			ActionToPerform=VoiceCommandAction.ZeroTwoFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero two six"],
			ActionToPerform=VoiceCommandAction.ZeroTwoSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero two seven"],
			ActionToPerform=VoiceCommandAction.ZeroTwoSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero two eight"],
			ActionToPerform=VoiceCommandAction.ZeroTwoEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero two nine"],
			ActionToPerform=VoiceCommandAction.ZeroTwoNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero three zero"],
			ActionToPerform=VoiceCommandAction.ZeroThreeZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero three one"],
			ActionToPerform=VoiceCommandAction.ZeroThreeOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero three two"],
			ActionToPerform=VoiceCommandAction.ZeroThreeTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero three three"],
			ActionToPerform=VoiceCommandAction.ZeroThreeThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero three four"],
			ActionToPerform=VoiceCommandAction.ZeroThreeFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero three five"],
			ActionToPerform=VoiceCommandAction.ZeroThreeFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero three six"],
			ActionToPerform=VoiceCommandAction.ZeroThreeSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero three seven"],
			ActionToPerform=VoiceCommandAction.ZeroThreeSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero three eight"],
			ActionToPerform=VoiceCommandAction.ZeroThreeEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero three nine"],
			ActionToPerform=VoiceCommandAction.ZeroThreeNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero four zero"],
			ActionToPerform=VoiceCommandAction.ZeroFourZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero four one"],
			ActionToPerform=VoiceCommandAction.ZeroFourOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero four two"],
			ActionToPerform=VoiceCommandAction.ZeroFourTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero four three"],
			ActionToPerform=VoiceCommandAction.ZeroFourThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero four four"],
			ActionToPerform=VoiceCommandAction.ZeroFourFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero four five"],
			ActionToPerform=VoiceCommandAction.ZeroFourFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero four six"],
			ActionToPerform=VoiceCommandAction.ZeroFourSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero four seven"],
			ActionToPerform=VoiceCommandAction.ZeroFourSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero four eight"],
			ActionToPerform=VoiceCommandAction.ZeroFourEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero four nine"],
			ActionToPerform=VoiceCommandAction.ZeroFourNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero five zero"],
			ActionToPerform=VoiceCommandAction.ZeroFiveZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero five one"],
			ActionToPerform=VoiceCommandAction.ZeroFiveOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero five two"],
			ActionToPerform=VoiceCommandAction.ZeroFiveTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero five three"],
			ActionToPerform=VoiceCommandAction.ZeroFiveThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero five four"],
			ActionToPerform=VoiceCommandAction.ZeroFiveFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero five five"],
			ActionToPerform=VoiceCommandAction.ZeroFiveFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero five six"],
			ActionToPerform=VoiceCommandAction.ZeroFiveSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero five seven"],
			ActionToPerform=VoiceCommandAction.ZeroFiveSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero five eight"],
			ActionToPerform=VoiceCommandAction.ZeroFiveEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero five nine"],
			ActionToPerform=VoiceCommandAction.ZeroFiveNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero six zero"],
			ActionToPerform=VoiceCommandAction.ZeroSixZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero six one"],
			ActionToPerform=VoiceCommandAction.ZeroSixOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero six two"],
			ActionToPerform=VoiceCommandAction.ZeroSixTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero six three"],
			ActionToPerform=VoiceCommandAction.ZeroSixThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero six four"],
			ActionToPerform=VoiceCommandAction.ZeroSixFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero six five"],
			ActionToPerform=VoiceCommandAction.ZeroSixFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero six six"],
			ActionToPerform=VoiceCommandAction.ZeroSixSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero six seven"],
			ActionToPerform=VoiceCommandAction.ZeroSixSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero six eight"],
			ActionToPerform=VoiceCommandAction.ZeroSixEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero six nine"],
			ActionToPerform=VoiceCommandAction.ZeroSixNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero seven zero"],
			ActionToPerform=VoiceCommandAction.ZeroSevenZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero seven one"],
			ActionToPerform=VoiceCommandAction.ZeroSevenOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero seven two"],
			ActionToPerform=VoiceCommandAction.ZeroSevenTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero seven three"],
			ActionToPerform=VoiceCommandAction.ZeroSevenThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero seven four"],
			ActionToPerform=VoiceCommandAction.ZeroSevenFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero seven five"],
			ActionToPerform=VoiceCommandAction.ZeroSevenFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero seven six"],
			ActionToPerform=VoiceCommandAction.ZeroSevenSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero seven seven"],
			ActionToPerform=VoiceCommandAction.ZeroSevenSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero seven eight"],
			ActionToPerform=VoiceCommandAction.ZeroSevenEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero seven nine"],
			ActionToPerform=VoiceCommandAction.ZeroSevenNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero eight zero"],
			ActionToPerform=VoiceCommandAction.ZeroEightZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero eight one"],
			ActionToPerform=VoiceCommandAction.ZeroEightOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero eight two"],
			ActionToPerform=VoiceCommandAction.ZeroEightTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero eight three"],
			ActionToPerform=VoiceCommandAction.ZeroEightThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero eight four"],
			ActionToPerform=VoiceCommandAction.ZeroEightFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero eight five"],
			ActionToPerform=VoiceCommandAction.ZeroEightFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero eight six"],
			ActionToPerform=VoiceCommandAction.ZeroEightSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero eight seven"],
			ActionToPerform=VoiceCommandAction.ZeroEightSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero eight eight"],
			ActionToPerform=VoiceCommandAction.ZeroEightEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero eight nine"],
			ActionToPerform=VoiceCommandAction.ZeroEightNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero nine zero"],
			ActionToPerform=VoiceCommandAction.ZeroNineZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero nine one"],
			ActionToPerform=VoiceCommandAction.ZeroNineOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero nine two"],
			ActionToPerform=VoiceCommandAction.ZeroNineTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero nine three"],
			ActionToPerform=VoiceCommandAction.ZeroNineThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero nine four"],
			ActionToPerform=VoiceCommandAction.ZeroNineFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero nine five"],
			ActionToPerform=VoiceCommandAction.ZeroNineFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero nine six"],
			ActionToPerform=VoiceCommandAction.ZeroNineSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero nine seven"],
			ActionToPerform=VoiceCommandAction.ZeroNineSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero nine eight"],
			ActionToPerform=VoiceCommandAction.ZeroNineEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero nine nine"],
			ActionToPerform=VoiceCommandAction.ZeroNineNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one zero zero"],
			ActionToPerform=VoiceCommandAction.OneZeroZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one zero one"],
			ActionToPerform=VoiceCommandAction.OneZeroOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one zero two"],
			ActionToPerform=VoiceCommandAction.OneZeroTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one zero three"],
			ActionToPerform=VoiceCommandAction.OneZeroThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one zero four"],
			ActionToPerform=VoiceCommandAction.OneZeroFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one zero five"],
			ActionToPerform=VoiceCommandAction.OneZeroFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one zero six"],
			ActionToPerform=VoiceCommandAction.OneZeroSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one zero seven"],
			ActionToPerform=VoiceCommandAction.OneZeroSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one zero eight"],
			ActionToPerform=VoiceCommandAction.OneZeroEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one zero nine"],
			ActionToPerform=VoiceCommandAction.OneZeroNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one one zero"],
			ActionToPerform=VoiceCommandAction.OneOneZero,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="one one zero"//Otherwise it was pronouncing 1 as 'ohney'
		},
		new VoiceCommand {
			Commands= ["one one one"],
			ActionToPerform=VoiceCommandAction.OneOneOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one one two"],
			ActionToPerform=VoiceCommandAction.OneOneTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one one three"],
			ActionToPerform=VoiceCommandAction.OneOneThree,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="one one three"//Otherwise it was pronouncing 1 as 'ohney'
		},
		new VoiceCommand {
			Commands= ["one one four"],
			ActionToPerform=VoiceCommandAction.OneOneFour,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="one one four"//Otherwise it was pronouncing 1 as 'ohney'
		},
		new VoiceCommand {
			Commands= ["one one five"],
			ActionToPerform=VoiceCommandAction.OneOneFive,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="one one five"//Otherwise it was pronouncing 1 as 'ohney'
		},
		new VoiceCommand {
			Commands= ["one one six"],
			ActionToPerform=VoiceCommandAction.OneOneSix,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="one one six"//Otherwise it was pronouncing 1 as 'ohney'
		},
		new VoiceCommand {
			Commands= ["one one seven"],
			ActionToPerform=VoiceCommandAction.OneOneSeven,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="one one seven"//Otherwise it was pronouncing 1 as 'ohney'
		},
		new VoiceCommand {
			Commands= ["one one eight"],
			ActionToPerform=VoiceCommandAction.OneOneEight,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="one one eight"//Otherwise it was pronouncing 1 as 'ohney'
		},
		new VoiceCommand {
			Commands= ["one one nine"],
			ActionToPerform=VoiceCommandAction.OneOneNine,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="one one nine"//Otherwise it was pronouncing 1 as 'ohney'
		},
		new VoiceCommand {
			Commands= ["one two zero"],
			ActionToPerform=VoiceCommandAction.OneTwoZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one two one"],
			ActionToPerform=VoiceCommandAction.OneTwoOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one two two"],
			ActionToPerform=VoiceCommandAction.OneTwoTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one two three"],
			ActionToPerform=VoiceCommandAction.OneTwoThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one two four"],
			ActionToPerform=VoiceCommandAction.OneTwoFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one two five"],
			ActionToPerform=VoiceCommandAction.OneTwoFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one two six"],
			ActionToPerform=VoiceCommandAction.OneTwoSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one two seven"],
			ActionToPerform=VoiceCommandAction.OneTwoSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one two eight"],
			ActionToPerform=VoiceCommandAction.OneTwoEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one two nine"],
			ActionToPerform=VoiceCommandAction.OneTwoNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one three zero"],
			ActionToPerform=VoiceCommandAction.OneThreeZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one three one"],
			ActionToPerform=VoiceCommandAction.OneThreeOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one three two"],
			ActionToPerform=VoiceCommandAction.OneThreeTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one three three"],
			ActionToPerform=VoiceCommandAction.OneThreeThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one three four"],
			ActionToPerform=VoiceCommandAction.OneThreeFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one three five"],
			ActionToPerform=VoiceCommandAction.OneThreeFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one three six"],
			ActionToPerform=VoiceCommandAction.OneThreeSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one three seven"],
			ActionToPerform=VoiceCommandAction.OneThreeSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one three eight"],
			ActionToPerform=VoiceCommandAction.OneThreeEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one three nine"],
			ActionToPerform=VoiceCommandAction.OneThreeNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one four zero"],
			ActionToPerform=VoiceCommandAction.OneFourZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one four one"],
			ActionToPerform=VoiceCommandAction.OneFourOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one four two"],
			ActionToPerform=VoiceCommandAction.OneFourTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one four three"],
			ActionToPerform=VoiceCommandAction.OneFourThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one four four"],
			ActionToPerform=VoiceCommandAction.OneFourFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one four five"],
			ActionToPerform=VoiceCommandAction.OneFourFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one four six"],
			ActionToPerform=VoiceCommandAction.OneFourSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one four seven"],
			ActionToPerform=VoiceCommandAction.OneFourSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one four eight"],
			ActionToPerform=VoiceCommandAction.OneFourEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one four nine"],
			ActionToPerform=VoiceCommandAction.OneFourNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one five zero"],
			ActionToPerform=VoiceCommandAction.OneFiveZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one five one"],
			ActionToPerform=VoiceCommandAction.OneFiveOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one five two"],
			ActionToPerform=VoiceCommandAction.OneFiveTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one five three"],
			ActionToPerform=VoiceCommandAction.OneFiveThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one five four"],
			ActionToPerform=VoiceCommandAction.OneFiveFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one five five"],
			ActionToPerform=VoiceCommandAction.OneFiveFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one five six"],
			ActionToPerform=VoiceCommandAction.OneFiveSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one five seven"],
			ActionToPerform=VoiceCommandAction.OneFiveSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one five eight"],
			ActionToPerform=VoiceCommandAction.OneFiveEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one five nine"],
			ActionToPerform=VoiceCommandAction.OneFiveNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one six zero"],
			ActionToPerform=VoiceCommandAction.OneSixZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one six one"],
			ActionToPerform=VoiceCommandAction.OneSixOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one six two"],
			ActionToPerform=VoiceCommandAction.OneSixTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one six three"],
			ActionToPerform=VoiceCommandAction.OneSixThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one six four"],
			ActionToPerform=VoiceCommandAction.OneSixFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one six five"],
			ActionToPerform=VoiceCommandAction.OneSixFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one six six"],
			ActionToPerform=VoiceCommandAction.OneSixSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one six seven"],
			ActionToPerform=VoiceCommandAction.OneSixSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one six eight"],
			ActionToPerform=VoiceCommandAction.OneSixEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one six nine"],
			ActionToPerform=VoiceCommandAction.OneSixNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one seven zero"],
			ActionToPerform=VoiceCommandAction.OneSevenZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one seven one"],
			ActionToPerform=VoiceCommandAction.OneSevenOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one seven two"],
			ActionToPerform=VoiceCommandAction.OneSevenTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one seven three"],
			ActionToPerform=VoiceCommandAction.OneSevenThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one seven four"],
			ActionToPerform=VoiceCommandAction.OneSevenFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one seven five"],
			ActionToPerform=VoiceCommandAction.OneSevenFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one seven six"],
			ActionToPerform=VoiceCommandAction.OneSevenSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one seven seven"],
			ActionToPerform=VoiceCommandAction.OneSevenSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one seven eight"],
			ActionToPerform=VoiceCommandAction.OneSevenEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one seven nine"],
			ActionToPerform=VoiceCommandAction.OneSevenNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one eight zero"],
			ActionToPerform=VoiceCommandAction.OneEightZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one eight one"],
			ActionToPerform=VoiceCommandAction.OneEightOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one eight two"],
			ActionToPerform=VoiceCommandAction.OneEightTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one eight three"],
			ActionToPerform=VoiceCommandAction.OneEightThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one eight four"],
			ActionToPerform=VoiceCommandAction.OneEightFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one eight five"],
			ActionToPerform=VoiceCommandAction.OneEightFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one eight six"],
			ActionToPerform=VoiceCommandAction.OneEightSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one eight seven"],
			ActionToPerform=VoiceCommandAction.OneEightSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one eight eight"],
			ActionToPerform=VoiceCommandAction.OneEightEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one eight nine"],
			ActionToPerform=VoiceCommandAction.OneEightNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one nine zero"],
			ActionToPerform=VoiceCommandAction.OneNineZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one nine one"],
			ActionToPerform=VoiceCommandAction.OneNineOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one nine two"],
			ActionToPerform=VoiceCommandAction.OneNineTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one nine three"],
			ActionToPerform=VoiceCommandAction.OneNineThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one nine four"],
			ActionToPerform=VoiceCommandAction.OneNineFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one nine five"],
			ActionToPerform=VoiceCommandAction.OneNineFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one nine six"],
			ActionToPerform=VoiceCommandAction.OneNineSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one nine seven"],
			ActionToPerform=VoiceCommandAction.OneNineSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one nine eight"],
			ActionToPerform=VoiceCommandAction.OneNineEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one nine nine"],
			ActionToPerform=VoiceCommandAction.OneNineNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two zero zero"],
			ActionToPerform=VoiceCommandAction.TwoZeroZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two zero one"],
			ActionToPerform=VoiceCommandAction.TwoZeroOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two zero two"],
			ActionToPerform=VoiceCommandAction.TwoZeroTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two zero three"],
			ActionToPerform=VoiceCommandAction.TwoZeroThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two zero four"],
			ActionToPerform=VoiceCommandAction.TwoZeroFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two zero five"],
			ActionToPerform=VoiceCommandAction.TwoZeroFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two zero six"],
			ActionToPerform=VoiceCommandAction.TwoZeroSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two zero seven"],
			ActionToPerform=VoiceCommandAction.TwoZeroSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two zero eight"],
			ActionToPerform=VoiceCommandAction.TwoZeroEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two zero nine"],
			ActionToPerform=VoiceCommandAction.TwoZeroNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two one zero"],
			ActionToPerform=VoiceCommandAction.TwoOneZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two one one"],
			ActionToPerform=VoiceCommandAction.TwoOneOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two one three"],
			ActionToPerform=VoiceCommandAction.TwoOneThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two one four"],
			ActionToPerform=VoiceCommandAction.TwoOneFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two one five"],
			ActionToPerform=VoiceCommandAction.TwoOneFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two one six"],
			ActionToPerform=VoiceCommandAction.TwoOneSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two one seven"],
			ActionToPerform=VoiceCommandAction.TwoOneSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two one eight"],
			ActionToPerform=VoiceCommandAction.TwoOneEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two one nine"],
			ActionToPerform=VoiceCommandAction.TwoOneNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two two zero"],
			ActionToPerform=VoiceCommandAction.TwoTwoZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two two one"],
			ActionToPerform=VoiceCommandAction.TwoTwoOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two two four"],
			ActionToPerform=VoiceCommandAction.TwoTwoFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two two five"],
			ActionToPerform=VoiceCommandAction.TwoTwoFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two two six"],
			ActionToPerform=VoiceCommandAction.TwoTwoSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two two seven"],
			ActionToPerform=VoiceCommandAction.TwoTwoSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two two eight"],
			ActionToPerform=VoiceCommandAction.TwoTwoEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two two nine"],
			ActionToPerform=VoiceCommandAction.TwoTwoNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two three zero"],
			ActionToPerform=VoiceCommandAction.TwoThreeZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two three one"],
			ActionToPerform=VoiceCommandAction.TwoThreeOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two three two"],
			ActionToPerform=VoiceCommandAction.TwoThreeTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two three three"],
			ActionToPerform=VoiceCommandAction.TwoThreeThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two three four"],
			ActionToPerform=VoiceCommandAction.TwoThreeFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two three five"],
			ActionToPerform=VoiceCommandAction.TwoThreeFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two three six"],
			ActionToPerform=VoiceCommandAction.TwoThreeSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two three seven"],
			ActionToPerform=VoiceCommandAction.TwoThreeSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two three eight"],
			ActionToPerform=VoiceCommandAction.TwoThreeEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two three nine"],
			ActionToPerform=VoiceCommandAction.TwoThreeNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two four zero"],
			ActionToPerform=VoiceCommandAction.TwoFourZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two four one"],
			ActionToPerform=VoiceCommandAction.TwoFourOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two four two"],
			ActionToPerform=VoiceCommandAction.TwoFourTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two four three"],
			ActionToPerform=VoiceCommandAction.TwoFourThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two four four"],
			ActionToPerform=VoiceCommandAction.TwoFourFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two four five"],
			ActionToPerform=VoiceCommandAction.TwoFourFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two four six"],
			ActionToPerform=VoiceCommandAction.TwoFourSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two four seven"],
			ActionToPerform=VoiceCommandAction.TwoFourSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two four eight"],
			ActionToPerform=VoiceCommandAction.TwoFourEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two four nine"],
			ActionToPerform=VoiceCommandAction.TwoFourNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two five zero"],
			ActionToPerform=VoiceCommandAction.TwoFiveZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two five one"],
			ActionToPerform=VoiceCommandAction.TwoFiveOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two five two"],
			ActionToPerform=VoiceCommandAction.TwoFiveTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two five three"],
			ActionToPerform=VoiceCommandAction.TwoFiveThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two five four"],
			ActionToPerform=VoiceCommandAction.TwoFiveFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two five five"],
			ActionToPerform=VoiceCommandAction.TwoFiveFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two five six"],
			ActionToPerform=VoiceCommandAction.TwoFiveSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two five seven"],
			ActionToPerform=VoiceCommandAction.TwoFiveSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two five eight"],
			ActionToPerform=VoiceCommandAction.TwoFiveEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two five nine"],
			ActionToPerform=VoiceCommandAction.TwoFiveNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two six zero"],
			ActionToPerform=VoiceCommandAction.TwoSixZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two six one"],
			ActionToPerform=VoiceCommandAction.TwoSixOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two six two"],
			ActionToPerform=VoiceCommandAction.TwoSixTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two six three"],
			ActionToPerform=VoiceCommandAction.TwoSixThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two six four"],
			ActionToPerform=VoiceCommandAction.TwoSixFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two six five"],
			ActionToPerform=VoiceCommandAction.TwoSixFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two six six"],
			ActionToPerform=VoiceCommandAction.TwoSixSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two six seven"],
			ActionToPerform=VoiceCommandAction.TwoSixSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two six eight"],
			ActionToPerform=VoiceCommandAction.TwoSixEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two six nine"],
			ActionToPerform=VoiceCommandAction.TwoSixNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two seven zero"],
			ActionToPerform=VoiceCommandAction.TwoSevenZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two seven one"],
			ActionToPerform=VoiceCommandAction.TwoSevenOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two seven two"],
			ActionToPerform=VoiceCommandAction.TwoSevenTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two seven three"],
			ActionToPerform=VoiceCommandAction.TwoSevenThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two seven four"],
			ActionToPerform=VoiceCommandAction.TwoSevenFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two seven five"],
			ActionToPerform=VoiceCommandAction.TwoSevenFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two seven six"],
			ActionToPerform=VoiceCommandAction.TwoSevenSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two seven seven"],
			ActionToPerform=VoiceCommandAction.TwoSevenSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two seven eight"],
			ActionToPerform=VoiceCommandAction.TwoSevenEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two seven nine"],
			ActionToPerform=VoiceCommandAction.TwoSevenNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two eight zero"],
			ActionToPerform=VoiceCommandAction.TwoEightZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two eight one"],
			ActionToPerform=VoiceCommandAction.TwoEightOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two eight two"],
			ActionToPerform=VoiceCommandAction.TwoEightTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two eight three"],
			ActionToPerform=VoiceCommandAction.TwoEightThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two eight four"],
			ActionToPerform=VoiceCommandAction.TwoEightFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two eight five"],
			ActionToPerform=VoiceCommandAction.TwoEightFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two eight six"],
			ActionToPerform=VoiceCommandAction.TwoEightSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two eight seven"],
			ActionToPerform=VoiceCommandAction.TwoEightSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two eight eight"],
			ActionToPerform=VoiceCommandAction.TwoEightEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two eight nine"],
			ActionToPerform=VoiceCommandAction.TwoEightNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two nine zero"],
			ActionToPerform=VoiceCommandAction.TwoNineZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two nine one"],
			ActionToPerform=VoiceCommandAction.TwoNineOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two nine two"],
			ActionToPerform=VoiceCommandAction.TwoNineTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two nine three"],
			ActionToPerform=VoiceCommandAction.TwoNineThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two nine four"],
			ActionToPerform=VoiceCommandAction.TwoNineFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two nine five"],
			ActionToPerform=VoiceCommandAction.TwoNineFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two nine six"],
			ActionToPerform=VoiceCommandAction.TwoNineSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two nine seven"],
			ActionToPerform=VoiceCommandAction.TwoNineSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two nine eight"],
			ActionToPerform=VoiceCommandAction.TwoNineEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two nine nine"],
			ActionToPerform=VoiceCommandAction.TwoNineNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three zero zero"],
			ActionToPerform=VoiceCommandAction.ThreeZeroZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three zero one"],
			ActionToPerform=VoiceCommandAction.ThreeZeroOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three zero two"],
			ActionToPerform=VoiceCommandAction.ThreeZeroTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three zero three"],
			ActionToPerform=VoiceCommandAction.ThreeZeroThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three zero four"],
			ActionToPerform=VoiceCommandAction.ThreeZeroFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three zero five"],
			ActionToPerform=VoiceCommandAction.ThreeZeroFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three zero six"],
			ActionToPerform=VoiceCommandAction.ThreeZeroSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three zero seven"],
			ActionToPerform=VoiceCommandAction.ThreeZeroSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three zero eight"],
			ActionToPerform=VoiceCommandAction.ThreeZeroEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three zero nine"],
			ActionToPerform=VoiceCommandAction.ThreeZeroNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three one zero"],
			ActionToPerform=VoiceCommandAction.ThreeOneZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three one one"],
			ActionToPerform=VoiceCommandAction.ThreeOneOne,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="three one one"//Otherwise it was pronouncing 1 as 'ohney'
		},
		new VoiceCommand {
			Commands= ["three one two"],
			ActionToPerform=VoiceCommandAction.ThreeOneTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three one three"],
			ActionToPerform=VoiceCommandAction.ThreeOneThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three one four"],
			ActionToPerform=VoiceCommandAction.ThreeOneFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three one five"],
			ActionToPerform=VoiceCommandAction.ThreeOneFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three one six"],
			ActionToPerform=VoiceCommandAction.ThreeOneSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three one seven"],
			ActionToPerform=VoiceCommandAction.ThreeOneSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three one eight"],
			ActionToPerform=VoiceCommandAction.ThreeOneEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three one nine"],
			ActionToPerform=VoiceCommandAction.ThreeOneNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three two zero"],
			ActionToPerform=VoiceCommandAction.ThreeTwoZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three two one"],
			ActionToPerform=VoiceCommandAction.ThreeTwoOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three two four"],
			ActionToPerform=VoiceCommandAction.ThreeTwoFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three two five"],
			ActionToPerform=VoiceCommandAction.ThreeTwoFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three two six"],
			ActionToPerform=VoiceCommandAction.ThreeTwoSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three two seven"],
			ActionToPerform=VoiceCommandAction.ThreeTwoSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three two eight"],
			ActionToPerform=VoiceCommandAction.ThreeTwoEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three two nine"],
			ActionToPerform=VoiceCommandAction.ThreeTwoNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three three zero"],
			ActionToPerform=VoiceCommandAction.ThreeThreeZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three three one"],
			ActionToPerform=VoiceCommandAction.ThreeThreeOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three three two"],
			ActionToPerform=VoiceCommandAction.ThreeThreeTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three three six"],
			ActionToPerform=VoiceCommandAction.ThreeThreeSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three three seven"],
			ActionToPerform=VoiceCommandAction.ThreeThreeSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three three eight"],
			ActionToPerform=VoiceCommandAction.ThreeThreeEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three three nine"],
			ActionToPerform=VoiceCommandAction.ThreeThreeNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three four zero"],
			ActionToPerform=VoiceCommandAction.ThreeFourZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three four one"],
			ActionToPerform=VoiceCommandAction.ThreeFourOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three four two"],
			ActionToPerform=VoiceCommandAction.ThreeFourTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three four four"],
			ActionToPerform=VoiceCommandAction.ThreeFourFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three four five"],
			ActionToPerform=VoiceCommandAction.ThreeFourFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three four six"],
			ActionToPerform=VoiceCommandAction.ThreeFourSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three four seven"],
			ActionToPerform=VoiceCommandAction.ThreeFourSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three four eight"],
			ActionToPerform=VoiceCommandAction.ThreeFourEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three four nine"],
			ActionToPerform=VoiceCommandAction.ThreeFourNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three five zero"],
			ActionToPerform=VoiceCommandAction.ThreeFiveZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three five one"],
			ActionToPerform=VoiceCommandAction.ThreeFiveOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three five two"],
			ActionToPerform=VoiceCommandAction.ThreeFiveTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three five three"],
			ActionToPerform=VoiceCommandAction.ThreeFiveThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three five four"],
			ActionToPerform=VoiceCommandAction.ThreeFiveFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three five five"],
			ActionToPerform=VoiceCommandAction.ThreeFiveFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three five six"],
			ActionToPerform=VoiceCommandAction.ThreeFiveSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three five seven"],
			ActionToPerform=VoiceCommandAction.ThreeFiveSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three five eight"],
			ActionToPerform=VoiceCommandAction.ThreeFiveEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three five nine"],
			ActionToPerform=VoiceCommandAction.ThreeFiveNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three six zero"],
			ActionToPerform=VoiceCommandAction.ThreeSixZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three six one"],
			ActionToPerform=VoiceCommandAction.ThreeSixOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three six two"],
			ActionToPerform=VoiceCommandAction.ThreeSixTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three six three"],
			ActionToPerform=VoiceCommandAction.ThreeSixThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three six four"],
			ActionToPerform=VoiceCommandAction.ThreeSixFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three six five"],
			ActionToPerform=VoiceCommandAction.ThreeSixFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three six six"],
			ActionToPerform=VoiceCommandAction.ThreeSixSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three six seven"],
			ActionToPerform=VoiceCommandAction.ThreeSixSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three six eight"],
			ActionToPerform=VoiceCommandAction.ThreeSixEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three six nine"],
			ActionToPerform=VoiceCommandAction.ThreeSixNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three seven zero"],
			ActionToPerform=VoiceCommandAction.ThreeSevenZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three seven one"],
			ActionToPerform=VoiceCommandAction.ThreeSevenOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three seven two"],
			ActionToPerform=VoiceCommandAction.ThreeSevenTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three seven three"],
			ActionToPerform=VoiceCommandAction.ThreeSevenThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three seven four"],
			ActionToPerform=VoiceCommandAction.ThreeSevenFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three seven five"],
			ActionToPerform=VoiceCommandAction.ThreeSevenFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three seven six"],
			ActionToPerform=VoiceCommandAction.ThreeSevenSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three seven seven"],
			ActionToPerform=VoiceCommandAction.ThreeSevenSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three seven eight"],
			ActionToPerform=VoiceCommandAction.ThreeSevenEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three seven nine"],
			ActionToPerform=VoiceCommandAction.ThreeSevenNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three eight zero"],
			ActionToPerform=VoiceCommandAction.ThreeEightZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three eight one"],
			ActionToPerform=VoiceCommandAction.ThreeEightOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three eight two"],
			ActionToPerform=VoiceCommandAction.ThreeEightTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three eight three"],
			ActionToPerform=VoiceCommandAction.ThreeEightThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three eight four"],
			ActionToPerform=VoiceCommandAction.ThreeEightFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three eight five"],
			ActionToPerform=VoiceCommandAction.ThreeEightFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three eight six"],
			ActionToPerform=VoiceCommandAction.ThreeEightSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three eight seven"],
			ActionToPerform=VoiceCommandAction.ThreeEightSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three eight eight"],
			ActionToPerform=VoiceCommandAction.ThreeEightEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three eight nine"],
			ActionToPerform=VoiceCommandAction.ThreeEightNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three nine zero"],
			ActionToPerform=VoiceCommandAction.ThreeNineZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three nine one"],
			ActionToPerform=VoiceCommandAction.ThreeNineOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three nine two"],
			ActionToPerform=VoiceCommandAction.ThreeNineTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three nine three"],
			ActionToPerform=VoiceCommandAction.ThreeNineThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three nine four"],
			ActionToPerform=VoiceCommandAction.ThreeNineFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three nine five"],
			ActionToPerform=VoiceCommandAction.ThreeNineFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three nine six"],
			ActionToPerform=VoiceCommandAction.ThreeNineSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three nine seven"],
			ActionToPerform=VoiceCommandAction.ThreeNineSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three nine eight"],
			ActionToPerform=VoiceCommandAction.ThreeNineEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three nine nine"],
			ActionToPerform=VoiceCommandAction.ThreeNineNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four zero zero"],
			ActionToPerform=VoiceCommandAction.FourZeroZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four zero one"],
			ActionToPerform=VoiceCommandAction.FourZeroOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four zero two"],
			ActionToPerform=VoiceCommandAction.FourZeroTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four zero three"],
			ActionToPerform=VoiceCommandAction.FourZeroThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four zero four"],
			ActionToPerform=VoiceCommandAction.FourZeroFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four zero five"],
			ActionToPerform=VoiceCommandAction.FourZeroFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four zero six"],
			ActionToPerform=VoiceCommandAction.FourZeroSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four zero seven"],
			ActionToPerform=VoiceCommandAction.FourZeroSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four zero eight"],
			ActionToPerform=VoiceCommandAction.FourZeroEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four zero nine"],
			ActionToPerform=VoiceCommandAction.FourZeroNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four one zero"],
			ActionToPerform=VoiceCommandAction.FourOneZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four one one"],
			ActionToPerform=VoiceCommandAction.FourOneOne,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="four one one"//Otherwise it was pronouncing 1 as 'ohney'
		},
		new VoiceCommand {
			Commands= ["four one two"],
			ActionToPerform=VoiceCommandAction.FourOneTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four one three"],
			ActionToPerform=VoiceCommandAction.FourOneThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four one four"],
			ActionToPerform=VoiceCommandAction.FourOneFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four one five"],
			ActionToPerform=VoiceCommandAction.FourOneFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four one six"],
			ActionToPerform=VoiceCommandAction.FourOneSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four one seven"],
			ActionToPerform=VoiceCommandAction.FourOneSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four one eight"],
			ActionToPerform=VoiceCommandAction.FourOneEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four one nine"],
			ActionToPerform=VoiceCommandAction.FourOneNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four two zero"],
			ActionToPerform=VoiceCommandAction.FourTwoZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four two one"],
			ActionToPerform=VoiceCommandAction.FourTwoOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four two two"],
			ActionToPerform=VoiceCommandAction.FourTwoTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four two three"],
			ActionToPerform=VoiceCommandAction.FourTwoThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four two four"],
			ActionToPerform=VoiceCommandAction.FourTwoFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four two five"],
			ActionToPerform=VoiceCommandAction.FourTwoFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four two six"],
			ActionToPerform=VoiceCommandAction.FourTwoSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four two seven"],
			ActionToPerform=VoiceCommandAction.FourTwoSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four two eight"],
			ActionToPerform=VoiceCommandAction.FourTwoEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four two nine"],
			ActionToPerform=VoiceCommandAction.FourTwoNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four three zero"],
			ActionToPerform=VoiceCommandAction.FourThreeZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four three one"],
			ActionToPerform=VoiceCommandAction.FourThreeOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four three two"],
			ActionToPerform=VoiceCommandAction.FourThreeTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four three six"],
			ActionToPerform=VoiceCommandAction.FourThreeSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four three seven"],
			ActionToPerform=VoiceCommandAction.FourThreeSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four three eight"],
			ActionToPerform=VoiceCommandAction.FourThreeEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four three nine"],
			ActionToPerform=VoiceCommandAction.FourThreeNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four four zero"],
			ActionToPerform=VoiceCommandAction.FourFourZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four four one"],
			ActionToPerform=VoiceCommandAction.FourFourOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four four two"],
			ActionToPerform=VoiceCommandAction.FourFourTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four four three"],
			ActionToPerform=VoiceCommandAction.FourFourThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four four six"],
			ActionToPerform=VoiceCommandAction.FourFourSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four four seven"],
			ActionToPerform=VoiceCommandAction.FourFourSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four four eight"],
			ActionToPerform=VoiceCommandAction.FourFourEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four four nine"],
			ActionToPerform=VoiceCommandAction.FourFourNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four five zero"],
			ActionToPerform=VoiceCommandAction.FourFiveZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four five one"],
			ActionToPerform=VoiceCommandAction.FourFiveOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four five two"],
			ActionToPerform=VoiceCommandAction.FourFiveTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four five three"],
			ActionToPerform=VoiceCommandAction.FourFiveThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four five four"],
			ActionToPerform=VoiceCommandAction.FourFiveFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four five five"],
			ActionToPerform=VoiceCommandAction.FourFiveFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four five six"],
			ActionToPerform=VoiceCommandAction.FourFiveSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four five seven"],
			ActionToPerform=VoiceCommandAction.FourFiveSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four five eight"],
			ActionToPerform=VoiceCommandAction.FourFiveEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four five nine"],
			ActionToPerform=VoiceCommandAction.FourFiveNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four six zero"],
			ActionToPerform=VoiceCommandAction.FourSixZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four six one"],
			ActionToPerform=VoiceCommandAction.FourSixOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four six two"],
			ActionToPerform=VoiceCommandAction.FourSixTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four six three"],
			ActionToPerform=VoiceCommandAction.FourSixThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four six four"],
			ActionToPerform=VoiceCommandAction.FourSixFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four six five"],
			ActionToPerform=VoiceCommandAction.FourSixFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four six six"],
			ActionToPerform=VoiceCommandAction.FourSixSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four six seven"],
			ActionToPerform=VoiceCommandAction.FourSixSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four six eight"],
			ActionToPerform=VoiceCommandAction.FourSixEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four six nine"],
			ActionToPerform=VoiceCommandAction.FourSixNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four seven zero"],
			ActionToPerform=VoiceCommandAction.FourSevenZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four seven one"],
			ActionToPerform=VoiceCommandAction.FourSevenOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four seven two"],
			ActionToPerform=VoiceCommandAction.FourSevenTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four seven three"],
			ActionToPerform=VoiceCommandAction.FourSevenThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four seven four"],
			ActionToPerform=VoiceCommandAction.FourSevenFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four seven five"],
			ActionToPerform=VoiceCommandAction.FourSevenFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four seven six"],
			ActionToPerform=VoiceCommandAction.FourSevenSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four seven seven"],
			ActionToPerform=VoiceCommandAction.FourSevenSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four seven eight"],
			ActionToPerform=VoiceCommandAction.FourSevenEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four seven nine"],
			ActionToPerform=VoiceCommandAction.FourSevenNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four eight zero"],
			ActionToPerform=VoiceCommandAction.FourEightZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four eight one"],
			ActionToPerform=VoiceCommandAction.FourEightOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four eight two"],
			ActionToPerform=VoiceCommandAction.FourEightTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four eight three"],
			ActionToPerform=VoiceCommandAction.FourEightThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four eight four"],
			ActionToPerform=VoiceCommandAction.FourEightFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four eight five"],
			ActionToPerform=VoiceCommandAction.FourEightFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four eight six"],
			ActionToPerform=VoiceCommandAction.FourEightSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four eight seven"],
			ActionToPerform=VoiceCommandAction.FourEightSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four eight eight"],
			ActionToPerform=VoiceCommandAction.FourEightEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four eight nine"],
			ActionToPerform=VoiceCommandAction.FourEightNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four nine zero"],
			ActionToPerform=VoiceCommandAction.FourNineZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four nine one"],
			ActionToPerform=VoiceCommandAction.FourNineOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four nine two"],
			ActionToPerform=VoiceCommandAction.FourNineTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four nine three"],
			ActionToPerform=VoiceCommandAction.FourNineThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four nine four"],
			ActionToPerform=VoiceCommandAction.FourNineFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four nine five"],
			ActionToPerform=VoiceCommandAction.FourNineFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four nine six"],
			ActionToPerform=VoiceCommandAction.FourNineSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four nine seven"],
			ActionToPerform=VoiceCommandAction.FourNineSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four nine eight"],
			ActionToPerform=VoiceCommandAction.FourNineEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four nine nine"],
			ActionToPerform=VoiceCommandAction.FourNineNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five zero zero"],
			ActionToPerform=VoiceCommandAction.FiveZeroZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five zero one"],
			ActionToPerform=VoiceCommandAction.FiveZeroOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five zero two"],
			ActionToPerform=VoiceCommandAction.FiveZeroTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five zero three"],
			ActionToPerform=VoiceCommandAction.FiveZeroThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five zero four"],
			ActionToPerform=VoiceCommandAction.FiveZeroFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five zero five"],
			ActionToPerform=VoiceCommandAction.FiveZeroFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five zero six"],
			ActionToPerform=VoiceCommandAction.FiveZeroSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five zero seven"],
			ActionToPerform=VoiceCommandAction.FiveZeroSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five zero eight"],
			ActionToPerform=VoiceCommandAction.FiveZeroEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five zero nine"],
			ActionToPerform=VoiceCommandAction.FiveZeroNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five one zero"],
			ActionToPerform=VoiceCommandAction.FiveOneZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five one one"],
			ActionToPerform=VoiceCommandAction.FiveOneOne,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="five one one"//Otherwise it was pronouncing 1 as 'ohney'
		},
		new VoiceCommand {
			Commands= ["five one two"],
			ActionToPerform=VoiceCommandAction.FiveOneTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five one three"],
			ActionToPerform=VoiceCommandAction.FiveOneThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five one four"],
			ActionToPerform=VoiceCommandAction.FiveOneFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five one five"],
			ActionToPerform=VoiceCommandAction.FiveOneFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five one six"],
			ActionToPerform=VoiceCommandAction.FiveOneSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five one seven"],
			ActionToPerform=VoiceCommandAction.FiveOneSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five one eight"],
			ActionToPerform=VoiceCommandAction.FiveOneEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five one nine"],
			ActionToPerform=VoiceCommandAction.FiveOneNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five two zero"],
			ActionToPerform=VoiceCommandAction.FiveTwoZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five two one"],
			ActionToPerform=VoiceCommandAction.FiveTwoOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five two two"],
			ActionToPerform=VoiceCommandAction.FiveTwoTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five two three"],
			ActionToPerform=VoiceCommandAction.FiveTwoThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five two four"],
			ActionToPerform=VoiceCommandAction.FiveTwoFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five two five"],
			ActionToPerform=VoiceCommandAction.FiveTwoFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five two six"],
			ActionToPerform=VoiceCommandAction.FiveTwoSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five two seven"],
			ActionToPerform=VoiceCommandAction.FiveTwoSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five two eight"],
			ActionToPerform=VoiceCommandAction.FiveTwoEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five two nine"],
			ActionToPerform=VoiceCommandAction.FiveTwoNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five three zero"],
			ActionToPerform=VoiceCommandAction.FiveThreeZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five three one"],
			ActionToPerform=VoiceCommandAction.FiveThreeOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five three two"],
			ActionToPerform=VoiceCommandAction.FiveThreeTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five three six"],
			ActionToPerform=VoiceCommandAction.FiveThreeSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five three seven"],
			ActionToPerform=VoiceCommandAction.FiveThreeSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five three eight"],
			ActionToPerform=VoiceCommandAction.FiveThreeEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five three nine"],
			ActionToPerform=VoiceCommandAction.FiveThreeNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five four zero"],
			ActionToPerform=VoiceCommandAction.FiveFourZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five four one"],
			ActionToPerform=VoiceCommandAction.FiveFourOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five four two"],
			ActionToPerform=VoiceCommandAction.FiveFourTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five four three"],
			ActionToPerform=VoiceCommandAction.FiveFourThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five four six"],
			ActionToPerform=VoiceCommandAction.FiveFourSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five four seven"],
			ActionToPerform=VoiceCommandAction.FiveFourSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five four eight"],
			ActionToPerform=VoiceCommandAction.FiveFourEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five four nine"],
			ActionToPerform=VoiceCommandAction.FiveFourNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five five zero"],
			ActionToPerform=VoiceCommandAction.FiveFiveZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five five one"],
			ActionToPerform=VoiceCommandAction.FiveFiveOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five five two"],
			ActionToPerform=VoiceCommandAction.FiveFiveTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five five three"],
			ActionToPerform=VoiceCommandAction.FiveFiveThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five five four"],
			ActionToPerform=VoiceCommandAction.FiveFiveFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five five six"],
			ActionToPerform=VoiceCommandAction.FiveFiveSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five five seven"],
			ActionToPerform=VoiceCommandAction.FiveFiveSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five five eight"],
			ActionToPerform=VoiceCommandAction.FiveFiveEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five five nine"],
			ActionToPerform=VoiceCommandAction.FiveFiveNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five six zero"],
			ActionToPerform=VoiceCommandAction.FiveSixZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five six one"],
			ActionToPerform=VoiceCommandAction.FiveSixOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five six two"],
			ActionToPerform=VoiceCommandAction.FiveSixTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five six three"],
			ActionToPerform=VoiceCommandAction.FiveSixThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five six four"],
			ActionToPerform=VoiceCommandAction.FiveSixFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five six five"],
			ActionToPerform=VoiceCommandAction.FiveSixFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five six six"],
			ActionToPerform=VoiceCommandAction.FiveSixSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five six seven"],
			ActionToPerform=VoiceCommandAction.FiveSixSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five six eight"],
			ActionToPerform=VoiceCommandAction.FiveSixEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five six nine"],
			ActionToPerform=VoiceCommandAction.FiveSixNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five seven zero"],
			ActionToPerform=VoiceCommandAction.FiveSevenZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five seven one"],
			ActionToPerform=VoiceCommandAction.FiveSevenOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five seven two"],
			ActionToPerform=VoiceCommandAction.FiveSevenTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five seven three"],
			ActionToPerform=VoiceCommandAction.FiveSevenThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five seven four"],
			ActionToPerform=VoiceCommandAction.FiveSevenFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five seven five"],
			ActionToPerform=VoiceCommandAction.FiveSevenFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five seven six"],
			ActionToPerform=VoiceCommandAction.FiveSevenSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five seven seven"],
			ActionToPerform=VoiceCommandAction.FiveSevenSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five seven eight"],
			ActionToPerform=VoiceCommandAction.FiveSevenEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five seven nine"],
			ActionToPerform=VoiceCommandAction.FiveSevenNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five eight zero"],
			ActionToPerform=VoiceCommandAction.FiveEightZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five eight one"],
			ActionToPerform=VoiceCommandAction.FiveEightOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five eight two"],
			ActionToPerform=VoiceCommandAction.FiveEightTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five eight three"],
			ActionToPerform=VoiceCommandAction.FiveEightThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five eight four"],
			ActionToPerform=VoiceCommandAction.FiveEightFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five eight five"],
			ActionToPerform=VoiceCommandAction.FiveEightFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five eight six"],
			ActionToPerform=VoiceCommandAction.FiveEightSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five eight seven"],
			ActionToPerform=VoiceCommandAction.FiveEightSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five eight eight"],
			ActionToPerform=VoiceCommandAction.FiveEightEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five eight nine"],
			ActionToPerform=VoiceCommandAction.FiveEightNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five nine zero"],
			ActionToPerform=VoiceCommandAction.FiveNineZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five nine one"],
			ActionToPerform=VoiceCommandAction.FiveNineOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five nine two"],
			ActionToPerform=VoiceCommandAction.FiveNineTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five nine three"],
			ActionToPerform=VoiceCommandAction.FiveNineThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five nine four"],
			ActionToPerform=VoiceCommandAction.FiveNineFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five nine five"],
			ActionToPerform=VoiceCommandAction.FiveNineFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five nine six"],
			ActionToPerform=VoiceCommandAction.FiveNineSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five nine seven"],
			ActionToPerform=VoiceCommandAction.FiveNineSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five nine eight"],
			ActionToPerform=VoiceCommandAction.FiveNineEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five nine nine"],
			ActionToPerform=VoiceCommandAction.FiveNineNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six zero zero"],
			ActionToPerform=VoiceCommandAction.SixZeroZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six zero one"],
			ActionToPerform=VoiceCommandAction.SixZeroOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six zero two"],
			ActionToPerform=VoiceCommandAction.SixZeroTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six zero three"],
			ActionToPerform=VoiceCommandAction.SixZeroThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six zero four"],
			ActionToPerform=VoiceCommandAction.SixZeroFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six zero five"],
			ActionToPerform=VoiceCommandAction.SixZeroFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six zero six"],
			ActionToPerform=VoiceCommandAction.SixZeroSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six zero seven"],
			ActionToPerform=VoiceCommandAction.SixZeroSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six zero eight"],
			ActionToPerform=VoiceCommandAction.SixZeroEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six zero nine"],
			ActionToPerform=VoiceCommandAction.SixZeroNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six one zero"],
			ActionToPerform=VoiceCommandAction.SixOneZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six one one"],
			ActionToPerform=VoiceCommandAction.SixOneOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six one two"],
			ActionToPerform=VoiceCommandAction.SixOneTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six one three"],
			ActionToPerform=VoiceCommandAction.SixOneThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six one four"],
			ActionToPerform=VoiceCommandAction.SixOneFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six one five"],
			ActionToPerform=VoiceCommandAction.SixOneFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six one six"],
			ActionToPerform=VoiceCommandAction.SixOneSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six one seven"],
			ActionToPerform=VoiceCommandAction.SixOneSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six one eight"],
			ActionToPerform=VoiceCommandAction.SixOneEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six one nine"],
			ActionToPerform=VoiceCommandAction.SixOneNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six two zero"],
			ActionToPerform=VoiceCommandAction.SixTwoZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six two one"],
			ActionToPerform=VoiceCommandAction.SixTwoOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six two two"],
			ActionToPerform=VoiceCommandAction.SixTwoTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six two three"],
			ActionToPerform=VoiceCommandAction.SixTwoThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six two four"],
			ActionToPerform=VoiceCommandAction.SixTwoFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six two five"],
			ActionToPerform=VoiceCommandAction.SixTwoFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six two six"],
			ActionToPerform=VoiceCommandAction.SixTwoSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six two seven"],
			ActionToPerform=VoiceCommandAction.SixTwoSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six two eight"],
			ActionToPerform=VoiceCommandAction.SixTwoEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six two nine"],
			ActionToPerform=VoiceCommandAction.SixTwoNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six three zero"],
			ActionToPerform=VoiceCommandAction.SixThreeZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six three one"],
			ActionToPerform=VoiceCommandAction.SixThreeOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six three two"],
			ActionToPerform=VoiceCommandAction.SixThreeTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six three three"],
			ActionToPerform=VoiceCommandAction.SixThreeThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six three four"],
			ActionToPerform=VoiceCommandAction.SixThreeFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six three five"],
			ActionToPerform=VoiceCommandAction.SixThreeFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six three six"],
			ActionToPerform=VoiceCommandAction.SixThreeSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six three seven"],
			ActionToPerform=VoiceCommandAction.SixThreeSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six three eight"],
			ActionToPerform=VoiceCommandAction.SixThreeEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six three nine"],
			ActionToPerform=VoiceCommandAction.SixThreeNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six four zero"],
			ActionToPerform=VoiceCommandAction.SixFourZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six four one"],
			ActionToPerform=VoiceCommandAction.SixFourOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six four two"],
			ActionToPerform=VoiceCommandAction.SixFourTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six four three"],
			ActionToPerform=VoiceCommandAction.SixFourThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six four four"],
			ActionToPerform=VoiceCommandAction.SixFourFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six four five"],
			ActionToPerform=VoiceCommandAction.SixFourFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six four six"],
			ActionToPerform=VoiceCommandAction.SixFourSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six four seven"],
			ActionToPerform=VoiceCommandAction.SixFourSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six four eight"],
			ActionToPerform=VoiceCommandAction.SixFourEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six four nine"],
			ActionToPerform=VoiceCommandAction.SixFourNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six five zero"],
			ActionToPerform=VoiceCommandAction.SixFiveZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six five one"],
			ActionToPerform=VoiceCommandAction.SixFiveOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six five two"],
			ActionToPerform=VoiceCommandAction.SixFiveTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six five three"],
			ActionToPerform=VoiceCommandAction.SixFiveThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six five four"],
			ActionToPerform=VoiceCommandAction.SixFiveFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six five five"],
			ActionToPerform=VoiceCommandAction.SixFiveFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six five six"],
			ActionToPerform=VoiceCommandAction.SixFiveSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six five seven"],
			ActionToPerform=VoiceCommandAction.SixFiveSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six five eight"],
			ActionToPerform=VoiceCommandAction.SixFiveEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six five nine"],
			ActionToPerform=VoiceCommandAction.SixFiveNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six six zero"],
			ActionToPerform=VoiceCommandAction.SixSixZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six six one"],
			ActionToPerform=VoiceCommandAction.SixSixOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six six two"],
			ActionToPerform=VoiceCommandAction.SixSixTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six six three"],
			ActionToPerform=VoiceCommandAction.SixSixThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six six four"],
			ActionToPerform=VoiceCommandAction.SixSixFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six six five"],
			ActionToPerform=VoiceCommandAction.SixSixFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six six six"],
			ActionToPerform=VoiceCommandAction.SixSixSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six six seven"],
			ActionToPerform=VoiceCommandAction.SixSixSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six six eight"],
			ActionToPerform=VoiceCommandAction.SixSixEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six six nine"],
			ActionToPerform=VoiceCommandAction.SixSixNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six seven zero"],
			ActionToPerform=VoiceCommandAction.SixSevenZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six seven one"],
			ActionToPerform=VoiceCommandAction.SixSevenOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six seven two"],
			ActionToPerform=VoiceCommandAction.SixSevenTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six seven three"],
			ActionToPerform=VoiceCommandAction.SixSevenThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six seven four"],
			ActionToPerform=VoiceCommandAction.SixSevenFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six seven five"],
			ActionToPerform=VoiceCommandAction.SixSevenFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six seven six"],
			ActionToPerform=VoiceCommandAction.SixSevenSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six seven seven"],
			ActionToPerform=VoiceCommandAction.SixSevenSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six seven eight"],
			ActionToPerform=VoiceCommandAction.SixSevenEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six seven nine"],
			ActionToPerform=VoiceCommandAction.SixSevenNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six eight zero"],
			ActionToPerform=VoiceCommandAction.SixEightZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six eight one"],
			ActionToPerform=VoiceCommandAction.SixEightOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six eight two"],
			ActionToPerform=VoiceCommandAction.SixEightTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six eight three"],
			ActionToPerform=VoiceCommandAction.SixEightThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six eight four"],
			ActionToPerform=VoiceCommandAction.SixEightFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six eight five"],
			ActionToPerform=VoiceCommandAction.SixEightFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six eight six"],
			ActionToPerform=VoiceCommandAction.SixEightSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six eight seven"],
			ActionToPerform=VoiceCommandAction.SixEightSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six eight eight"],
			ActionToPerform=VoiceCommandAction.SixEightEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six eight nine"],
			ActionToPerform=VoiceCommandAction.SixEightNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six nine zero"],
			ActionToPerform=VoiceCommandAction.SixNineZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six nine one"],
			ActionToPerform=VoiceCommandAction.SixNineOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six nine two"],
			ActionToPerform=VoiceCommandAction.SixNineTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six nine three"],
			ActionToPerform=VoiceCommandAction.SixNineThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six nine four"],
			ActionToPerform=VoiceCommandAction.SixNineFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six nine five"],
			ActionToPerform=VoiceCommandAction.SixNineFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six nine six"],
			ActionToPerform=VoiceCommandAction.SixNineSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six nine seven"],
			ActionToPerform=VoiceCommandAction.SixNineSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six nine eight"],
			ActionToPerform=VoiceCommandAction.SixNineEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six nine nine"],
			ActionToPerform=VoiceCommandAction.SixNineNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven zero zero"],
			ActionToPerform=VoiceCommandAction.SevenZeroZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven zero one"],
			ActionToPerform=VoiceCommandAction.SevenZeroOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven zero two"],
			ActionToPerform=VoiceCommandAction.SevenZeroTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven zero three"],
			ActionToPerform=VoiceCommandAction.SevenZeroThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven zero four"],
			ActionToPerform=VoiceCommandAction.SevenZeroFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven zero five"],
			ActionToPerform=VoiceCommandAction.SevenZeroFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven zero six"],
			ActionToPerform=VoiceCommandAction.SevenZeroSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven zero seven"],
			ActionToPerform=VoiceCommandAction.SevenZeroSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven zero eight"],
			ActionToPerform=VoiceCommandAction.SevenZeroEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven zero nine"],
			ActionToPerform=VoiceCommandAction.SevenZeroNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven one zero"],
			ActionToPerform=VoiceCommandAction.SevenOneZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven one one"],
			ActionToPerform=VoiceCommandAction.SevenOneOne,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="seven one one"//Otherwise it was pronouncing 1 as 'ohney'
		},
		new VoiceCommand {
			Commands= ["seven one two"],
			ActionToPerform=VoiceCommandAction.SevenOneTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven one three"],
			ActionToPerform=VoiceCommandAction.SevenOneThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven one four"],
			ActionToPerform=VoiceCommandAction.SevenOneFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven one five"],
			ActionToPerform=VoiceCommandAction.SevenOneFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven one six"],
			ActionToPerform=VoiceCommandAction.SevenOneSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven one seven"],
			ActionToPerform=VoiceCommandAction.SevenOneSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven one eight"],
			ActionToPerform=VoiceCommandAction.SevenOneEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven one nine"],
			ActionToPerform=VoiceCommandAction.SevenOneNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven two zero"],
			ActionToPerform=VoiceCommandAction.SevenTwoZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven two one"],
			ActionToPerform=VoiceCommandAction.SevenTwoOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven two two"],
			ActionToPerform=VoiceCommandAction.SevenTwoTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven two three"],
			ActionToPerform=VoiceCommandAction.SevenTwoThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven two four"],
			ActionToPerform=VoiceCommandAction.SevenTwoFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven two five"],
			ActionToPerform=VoiceCommandAction.SevenTwoFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven two six"],
			ActionToPerform=VoiceCommandAction.SevenTwoSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven two seven"],
			ActionToPerform=VoiceCommandAction.SevenTwoSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven two eight"],
			ActionToPerform=VoiceCommandAction.SevenTwoEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven two nine"],
			ActionToPerform=VoiceCommandAction.SevenTwoNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven three zero"],
			ActionToPerform=VoiceCommandAction.SevenThreeZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven three one"],
			ActionToPerform=VoiceCommandAction.SevenThreeOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven three two"],
			ActionToPerform=VoiceCommandAction.SevenThreeTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven three three"],
			ActionToPerform=VoiceCommandAction.SevenThreeThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven three four"],
			ActionToPerform=VoiceCommandAction.SevenThreeFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven three five"],
			ActionToPerform=VoiceCommandAction.SevenThreeFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven three six"],
			ActionToPerform=VoiceCommandAction.SevenThreeSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven three seven"],
			ActionToPerform=VoiceCommandAction.SevenThreeSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven three eight"],
			ActionToPerform=VoiceCommandAction.SevenThreeEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven three nine"],
			ActionToPerform=VoiceCommandAction.SevenThreeNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven four zero"],
			ActionToPerform=VoiceCommandAction.SevenFourZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven four one"],
			ActionToPerform=VoiceCommandAction.SevenFourOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven four two"],
			ActionToPerform=VoiceCommandAction.SevenFourTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven four three"],
			ActionToPerform=VoiceCommandAction.SevenFourThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven four four"],
			ActionToPerform=VoiceCommandAction.SevenFourFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven four five"],
			ActionToPerform=VoiceCommandAction.SevenFourFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven four six"],
			ActionToPerform=VoiceCommandAction.SevenFourSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven four seven"],
			ActionToPerform=VoiceCommandAction.SevenFourSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven four eight"],
			ActionToPerform=VoiceCommandAction.SevenFourEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven four nine"],
			ActionToPerform=VoiceCommandAction.SevenFourNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven five zero"],
			ActionToPerform=VoiceCommandAction.SevenFiveZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven five one"],
			ActionToPerform=VoiceCommandAction.SevenFiveOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven five two"],
			ActionToPerform=VoiceCommandAction.SevenFiveTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven five three"],
			ActionToPerform=VoiceCommandAction.SevenFiveThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven five four"],
			ActionToPerform=VoiceCommandAction.SevenFiveFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven five five"],
			ActionToPerform=VoiceCommandAction.SevenFiveFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven five six"],
			ActionToPerform=VoiceCommandAction.SevenFiveSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven five seven"],
			ActionToPerform=VoiceCommandAction.SevenFiveSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven five eight"],
			ActionToPerform=VoiceCommandAction.SevenFiveEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven five nine"],
			ActionToPerform=VoiceCommandAction.SevenFiveNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven six zero"],
			ActionToPerform=VoiceCommandAction.SevenSixZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven six one"],
			ActionToPerform=VoiceCommandAction.SevenSixOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven six two"],
			ActionToPerform=VoiceCommandAction.SevenSixTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven six three"],
			ActionToPerform=VoiceCommandAction.SevenSixThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven six four"],
			ActionToPerform=VoiceCommandAction.SevenSixFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven six five"],
			ActionToPerform=VoiceCommandAction.SevenSixFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven six six"],
			ActionToPerform=VoiceCommandAction.SevenSixSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven six seven"],
			ActionToPerform=VoiceCommandAction.SevenSixSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven six eight"],
			ActionToPerform=VoiceCommandAction.SevenSixEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven six nine"],
			ActionToPerform=VoiceCommandAction.SevenSixNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven seven zero"],
			ActionToPerform=VoiceCommandAction.SevenSevenZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven seven one"],
			ActionToPerform=VoiceCommandAction.SevenSevenOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven seven two"],
			ActionToPerform=VoiceCommandAction.SevenSevenTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven seven three"],
			ActionToPerform=VoiceCommandAction.SevenSevenThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven seven four"],
			ActionToPerform=VoiceCommandAction.SevenSevenFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven seven five"],
			ActionToPerform=VoiceCommandAction.SevenSevenFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven seven six"],
			ActionToPerform=VoiceCommandAction.SevenSevenSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven seven seven"],
			ActionToPerform=VoiceCommandAction.SevenSevenSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven seven eight"],
			ActionToPerform=VoiceCommandAction.SevenSevenEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven seven nine"],
			ActionToPerform=VoiceCommandAction.SevenSevenNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven eight zero"],
			ActionToPerform=VoiceCommandAction.SevenEightZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven eight one"],
			ActionToPerform=VoiceCommandAction.SevenEightOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven eight two"],
			ActionToPerform=VoiceCommandAction.SevenEightTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven eight three"],
			ActionToPerform=VoiceCommandAction.SevenEightThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven eight four"],
			ActionToPerform=VoiceCommandAction.SevenEightFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven eight five"],
			ActionToPerform=VoiceCommandAction.SevenEightFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven eight six"],
			ActionToPerform=VoiceCommandAction.SevenEightSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven eight seven"],
			ActionToPerform=VoiceCommandAction.SevenEightSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven eight eight"],
			ActionToPerform=VoiceCommandAction.SevenEightEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven eight nine"],
			ActionToPerform=VoiceCommandAction.SevenEightNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven nine zero"],
			ActionToPerform=VoiceCommandAction.SevenNineZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven nine one"],
			ActionToPerform=VoiceCommandAction.SevenNineOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven nine two"],
			ActionToPerform=VoiceCommandAction.SevenNineTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven nine three"],
			ActionToPerform=VoiceCommandAction.SevenNineThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven nine four"],
			ActionToPerform=VoiceCommandAction.SevenNineFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven nine five"],
			ActionToPerform=VoiceCommandAction.SevenNineFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven nine six"],
			ActionToPerform=VoiceCommandAction.SevenNineSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven nine seven"],
			ActionToPerform=VoiceCommandAction.SevenNineSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven nine eight"],
			ActionToPerform=VoiceCommandAction.SevenNineEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven nine nine"],
			ActionToPerform=VoiceCommandAction.SevenNineNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight zero zero"],
			ActionToPerform=VoiceCommandAction.EightZeroZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight zero one"],
			ActionToPerform=VoiceCommandAction.EightZeroOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight zero two"],
			ActionToPerform=VoiceCommandAction.EightZeroTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight zero three"],
			ActionToPerform=VoiceCommandAction.EightZeroThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight zero four"],
			ActionToPerform=VoiceCommandAction.EightZeroFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight zero five"],
			ActionToPerform=VoiceCommandAction.EightZeroFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight zero six"],
			ActionToPerform=VoiceCommandAction.EightZeroSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight zero seven"],
			ActionToPerform=VoiceCommandAction.EightZeroSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight zero eight"],
			ActionToPerform=VoiceCommandAction.EightZeroEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight zero nine"],
			ActionToPerform=VoiceCommandAction.EightZeroNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight one zero"],
			ActionToPerform=VoiceCommandAction.EightOneZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight one one"],
			ActionToPerform=VoiceCommandAction.EightOneOne,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="eight one one"//Otherwise it was pronouncing 1 as 'ohney'
		},
		new VoiceCommand {
			Commands= ["eight one two"],
			ActionToPerform=VoiceCommandAction.EightOneTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight one three"],
			ActionToPerform=VoiceCommandAction.EightOneThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight one four"],
			ActionToPerform=VoiceCommandAction.EightOneFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight one five"],
			ActionToPerform=VoiceCommandAction.EightOneFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight one six"],
			ActionToPerform=VoiceCommandAction.EightOneSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight one seven"],
			ActionToPerform=VoiceCommandAction.EightOneSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight one eight"],
			ActionToPerform=VoiceCommandAction.EightOneEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight one nine"],
			ActionToPerform=VoiceCommandAction.EightOneNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight two zero"],
			ActionToPerform=VoiceCommandAction.EightTwoZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight two one"],
			ActionToPerform=VoiceCommandAction.EightTwoOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight two two"],
			ActionToPerform=VoiceCommandAction.EightTwoTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight two three"],
			ActionToPerform=VoiceCommandAction.EightTwoThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight two four"],
			ActionToPerform=VoiceCommandAction.EightTwoFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight two five"],
			ActionToPerform=VoiceCommandAction.EightTwoFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight two six"],
			ActionToPerform=VoiceCommandAction.EightTwoSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight two seven"],
			ActionToPerform=VoiceCommandAction.EightTwoSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight two eight"],
			ActionToPerform=VoiceCommandAction.EightTwoEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight two nine"],
			ActionToPerform=VoiceCommandAction.EightTwoNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight three zero"],
			ActionToPerform=VoiceCommandAction.EightThreeZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight three one"],
			ActionToPerform=VoiceCommandAction.EightThreeOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight three two"],
			ActionToPerform=VoiceCommandAction.EightThreeTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight three three"],
			ActionToPerform=VoiceCommandAction.EightThreeThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight three four"],
			ActionToPerform=VoiceCommandAction.EightThreeFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight three five"],
			ActionToPerform=VoiceCommandAction.EightThreeFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight three six"],
			ActionToPerform=VoiceCommandAction.EightThreeSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight three seven"],
			ActionToPerform=VoiceCommandAction.EightThreeSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight three eight"],
			ActionToPerform=VoiceCommandAction.EightThreeEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight three nine"],
			ActionToPerform=VoiceCommandAction.EightThreeNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight four zero"],
			ActionToPerform=VoiceCommandAction.EightFourZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight four one"],
			ActionToPerform=VoiceCommandAction.EightFourOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight four two"],
			ActionToPerform=VoiceCommandAction.EightFourTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight four three"],
			ActionToPerform=VoiceCommandAction.EightFourThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight four four"],
			ActionToPerform=VoiceCommandAction.EightFourFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight four five"],
			ActionToPerform=VoiceCommandAction.EightFourFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight four six"],
			ActionToPerform=VoiceCommandAction.EightFourSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight four seven"],
			ActionToPerform=VoiceCommandAction.EightFourSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight four eight"],
			ActionToPerform=VoiceCommandAction.EightFourEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight four nine"],
			ActionToPerform=VoiceCommandAction.EightFourNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight five zero"],
			ActionToPerform=VoiceCommandAction.EightFiveZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight five one"],
			ActionToPerform=VoiceCommandAction.EightFiveOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight five two"],
			ActionToPerform=VoiceCommandAction.EightFiveTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight five three"],
			ActionToPerform=VoiceCommandAction.EightFiveThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight five four"],
			ActionToPerform=VoiceCommandAction.EightFiveFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight five five"],
			ActionToPerform=VoiceCommandAction.EightFiveFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight five six"],
			ActionToPerform=VoiceCommandAction.EightFiveSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight five seven"],
			ActionToPerform=VoiceCommandAction.EightFiveSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight five eight"],
			ActionToPerform=VoiceCommandAction.EightFiveEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight five nine"],
			ActionToPerform=VoiceCommandAction.EightFiveNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight six zero"],
			ActionToPerform=VoiceCommandAction.EightSixZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight six one"],
			ActionToPerform=VoiceCommandAction.EightSixOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight six two"],
			ActionToPerform=VoiceCommandAction.EightSixTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight six three"],
			ActionToPerform=VoiceCommandAction.EightSixThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight six four"],
			ActionToPerform=VoiceCommandAction.EightSixFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight six five"],
			ActionToPerform=VoiceCommandAction.EightSixFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight six six"],
			ActionToPerform=VoiceCommandAction.EightSixSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight six seven"],
			ActionToPerform=VoiceCommandAction.EightSixSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight six eight"],
			ActionToPerform=VoiceCommandAction.EightSixEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight six nine"],
			ActionToPerform=VoiceCommandAction.EightSixNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight seven zero"],
			ActionToPerform=VoiceCommandAction.EightSevenZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight seven one"],
			ActionToPerform=VoiceCommandAction.EightSevenOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight seven two"],
			ActionToPerform=VoiceCommandAction.EightSevenTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight seven three"],
			ActionToPerform=VoiceCommandAction.EightSevenThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight seven four"],
			ActionToPerform=VoiceCommandAction.EightSevenFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight seven five"],
			ActionToPerform=VoiceCommandAction.EightSevenFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight seven six"],
			ActionToPerform=VoiceCommandAction.EightSevenSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight seven seven"],
			ActionToPerform=VoiceCommandAction.EightSevenSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight seven eight"],
			ActionToPerform=VoiceCommandAction.EightSevenEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight seven nine"],
			ActionToPerform=VoiceCommandAction.EightSevenNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight eight zero"],
			ActionToPerform=VoiceCommandAction.EightEightZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight eight one"],
			ActionToPerform=VoiceCommandAction.EightEightOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight eight two"],
			ActionToPerform=VoiceCommandAction.EightEightTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight eight three"],
			ActionToPerform=VoiceCommandAction.EightEightThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight eight four"],
			ActionToPerform=VoiceCommandAction.EightEightFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight eight five"],
			ActionToPerform=VoiceCommandAction.EightEightFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight eight six"],
			ActionToPerform=VoiceCommandAction.EightEightSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight eight seven"],
			ActionToPerform=VoiceCommandAction.EightEightSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight eight eight"],
			ActionToPerform=VoiceCommandAction.EightEightEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight eight nine"],
			ActionToPerform=VoiceCommandAction.EightEightNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight nine zero"],
			ActionToPerform=VoiceCommandAction.EightNineZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight nine one"],
			ActionToPerform=VoiceCommandAction.EightNineOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight nine two"],
			ActionToPerform=VoiceCommandAction.EightNineTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight nine three"],
			ActionToPerform=VoiceCommandAction.EightNineThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight nine four"],
			ActionToPerform=VoiceCommandAction.EightNineFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight nine five"],
			ActionToPerform=VoiceCommandAction.EightNineFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight nine six"],
			ActionToPerform=VoiceCommandAction.EightNineSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight nine seven"],
			ActionToPerform=VoiceCommandAction.EightNineSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight nine eight"],
			ActionToPerform=VoiceCommandAction.EightNineEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight nine nine"],
			ActionToPerform=VoiceCommandAction.EightNineNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine zero zero"],
			ActionToPerform=VoiceCommandAction.NineZeroZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine zero one"],
			ActionToPerform=VoiceCommandAction.NineZeroOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine zero two"],
			ActionToPerform=VoiceCommandAction.NineZeroTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine zero three"],
			ActionToPerform=VoiceCommandAction.NineZeroThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine zero four"],
			ActionToPerform=VoiceCommandAction.NineZeroFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine zero five"],
			ActionToPerform=VoiceCommandAction.NineZeroFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine zero six"],
			ActionToPerform=VoiceCommandAction.NineZeroSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine zero seven"],
			ActionToPerform=VoiceCommandAction.NineZeroSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine zero eight"],
			ActionToPerform=VoiceCommandAction.NineZeroEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine zero nine"],
			ActionToPerform=VoiceCommandAction.NineZeroNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine one zero"],
			ActionToPerform=VoiceCommandAction.NineOneZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine one one"],
			ActionToPerform=VoiceCommandAction.NineOneOne,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="nine one one"//Otherwise it was pronouncing 1 as 'ohney'
		},
		new VoiceCommand {
			Commands= ["nine one two"],
			ActionToPerform=VoiceCommandAction.NineOneTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine one three"],
			ActionToPerform=VoiceCommandAction.NineOneThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine one four"],
			ActionToPerform=VoiceCommandAction.NineOneFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine one five"],
			ActionToPerform=VoiceCommandAction.NineOneFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine one six"],
			ActionToPerform=VoiceCommandAction.NineOneSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine one seven"],
			ActionToPerform=VoiceCommandAction.NineOneSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine one eight"],
			ActionToPerform=VoiceCommandAction.NineOneEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine one nine"],
			ActionToPerform=VoiceCommandAction.NineOneNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine two zero"],
			ActionToPerform=VoiceCommandAction.NineTwoZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine two one"],
			ActionToPerform=VoiceCommandAction.NineTwoOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine two two"],
			ActionToPerform=VoiceCommandAction.NineTwoTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine two three"],
			ActionToPerform=VoiceCommandAction.NineTwoThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine two four"],
			ActionToPerform=VoiceCommandAction.NineTwoFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine two five"],
			ActionToPerform=VoiceCommandAction.NineTwoFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine two six"],
			ActionToPerform=VoiceCommandAction.NineTwoSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine two seven"],
			ActionToPerform=VoiceCommandAction.NineTwoSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine two eight"],
			ActionToPerform=VoiceCommandAction.NineTwoEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine two nine"],
			ActionToPerform=VoiceCommandAction.NineTwoNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine three zero"],
			ActionToPerform=VoiceCommandAction.NineThreeZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine three one"],
			ActionToPerform=VoiceCommandAction.NineThreeOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine three two"],
			ActionToPerform=VoiceCommandAction.NineThreeTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine three three"],
			ActionToPerform=VoiceCommandAction.NineThreeThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine three four"],
			ActionToPerform=VoiceCommandAction.NineThreeFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine three five"],
			ActionToPerform=VoiceCommandAction.NineThreeFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine three six"],
			ActionToPerform=VoiceCommandAction.NineThreeSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine three seven"],
			ActionToPerform=VoiceCommandAction.NineThreeSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine three eight"],
			ActionToPerform=VoiceCommandAction.NineThreeEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine three nine"],
			ActionToPerform=VoiceCommandAction.NineThreeNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine four zero"],
			ActionToPerform=VoiceCommandAction.NineFourZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine four one"],
			ActionToPerform=VoiceCommandAction.NineFourOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine four two"],
			ActionToPerform=VoiceCommandAction.NineFourTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine four three"],
			ActionToPerform=VoiceCommandAction.NineFourThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine four four"],
			ActionToPerform=VoiceCommandAction.NineFourFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine four five"],
			ActionToPerform=VoiceCommandAction.NineFourFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine four six"],
			ActionToPerform=VoiceCommandAction.NineFourSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine four seven"],
			ActionToPerform=VoiceCommandAction.NineFourSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine four eight"],
			ActionToPerform=VoiceCommandAction.NineFourEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine four nine"],
			ActionToPerform=VoiceCommandAction.NineFourNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine five zero"],
			ActionToPerform=VoiceCommandAction.NineFiveZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine five one"],
			ActionToPerform=VoiceCommandAction.NineFiveOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine five two"],
			ActionToPerform=VoiceCommandAction.NineFiveTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine five three"],
			ActionToPerform=VoiceCommandAction.NineFiveThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine five four"],
			ActionToPerform=VoiceCommandAction.NineFiveFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine five five"],
			ActionToPerform=VoiceCommandAction.NineFiveFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine five six"],
			ActionToPerform=VoiceCommandAction.NineFiveSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine five seven"],
			ActionToPerform=VoiceCommandAction.NineFiveSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine five eight"],
			ActionToPerform=VoiceCommandAction.NineFiveEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine five nine"],
			ActionToPerform=VoiceCommandAction.NineFiveNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine six zero"],
			ActionToPerform=VoiceCommandAction.NineSixZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine six one"],
			ActionToPerform=VoiceCommandAction.NineSixOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine six two"],
			ActionToPerform=VoiceCommandAction.NineSixTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine six three"],
			ActionToPerform=VoiceCommandAction.NineSixThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine six four"],
			ActionToPerform=VoiceCommandAction.NineSixFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine six five"],
			ActionToPerform=VoiceCommandAction.NineSixFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine six six"],
			ActionToPerform=VoiceCommandAction.NineSixSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine six seven"],
			ActionToPerform=VoiceCommandAction.NineSixSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine six eight"],
			ActionToPerform=VoiceCommandAction.NineSixEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine six nine"],
			ActionToPerform=VoiceCommandAction.NineSixNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine seven zero"],
			ActionToPerform=VoiceCommandAction.NineSevenZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine seven one"],
			ActionToPerform=VoiceCommandAction.NineSevenOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine seven two"],
			ActionToPerform=VoiceCommandAction.NineSevenTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine seven three"],
			ActionToPerform=VoiceCommandAction.NineSevenThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine seven four"],
			ActionToPerform=VoiceCommandAction.NineSevenFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine seven five"],
			ActionToPerform=VoiceCommandAction.NineSevenFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine seven six"],
			ActionToPerform=VoiceCommandAction.NineSevenSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine seven seven"],
			ActionToPerform=VoiceCommandAction.NineSevenSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine seven eight"],
			ActionToPerform=VoiceCommandAction.NineSevenEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine seven nine"],
			ActionToPerform=VoiceCommandAction.NineSevenNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine eight zero"],
			ActionToPerform=VoiceCommandAction.NineEightZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine eight one"],
			ActionToPerform=VoiceCommandAction.NineEightOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine eight two"],
			ActionToPerform=VoiceCommandAction.NineEightTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine eight three"],
			ActionToPerform=VoiceCommandAction.NineEightThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine eight four"],
			ActionToPerform=VoiceCommandAction.NineEightFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine eight five"],
			ActionToPerform=VoiceCommandAction.NineEightFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine eight six"],
			ActionToPerform=VoiceCommandAction.NineEightSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine eight seven"],
			ActionToPerform=VoiceCommandAction.NineEightSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine eight eight"],
			ActionToPerform=VoiceCommandAction.NineEightEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine eight nine"],
			ActionToPerform=VoiceCommandAction.NineEightNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine nine zero"],
			ActionToPerform=VoiceCommandAction.NineNineZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine nine one"],
			ActionToPerform=VoiceCommandAction.NineNineOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine nine two"],
			ActionToPerform=VoiceCommandAction.NineNineTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine nine three"],
			ActionToPerform=VoiceCommandAction.NineNineThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine nine four"],
			ActionToPerform=VoiceCommandAction.NineNineFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine nine five"],
			ActionToPerform=VoiceCommandAction.NineNineFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine nine six"],
			ActionToPerform=VoiceCommandAction.NineNineSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine nine seven"],
			ActionToPerform=VoiceCommandAction.NineNineSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine nine eight"],
			ActionToPerform=VoiceCommandAction.NineNineEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine nine nine"],
			ActionToPerform=VoiceCommandAction.NineNineNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero zero"],
			ActionToPerform=VoiceCommandAction.ZeroZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero one"],
			ActionToPerform=VoiceCommandAction.ZeroOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero two"],
			ActionToPerform=VoiceCommandAction.ZeroTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero three"],
			ActionToPerform=VoiceCommandAction.ZeroThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero four"],
			ActionToPerform=VoiceCommandAction.ZeroFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero five"],
			ActionToPerform=VoiceCommandAction.ZeroFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero six"],
			ActionToPerform=VoiceCommandAction.ZeroSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero seven"],
			ActionToPerform=VoiceCommandAction.ZeroSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero eight"],
			ActionToPerform=VoiceCommandAction.ZeroEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["zero nine"],
			ActionToPerform=VoiceCommandAction.ZeroNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one zero"],
			ActionToPerform=VoiceCommandAction.OneZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one one"],
			ActionToPerform=VoiceCommandAction.OneOne,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="one one",
		},
		new VoiceCommand {
			Commands= ["one two"],
			ActionToPerform=VoiceCommandAction.OneTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one three"],
			ActionToPerform=VoiceCommandAction.OneThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one four"],
			ActionToPerform=VoiceCommandAction.OneFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one five"],
			ActionToPerform=VoiceCommandAction.OneFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one six"],
			ActionToPerform=VoiceCommandAction.OneSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one seven"],
			ActionToPerform=VoiceCommandAction.OneSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one eight"],
			ActionToPerform=VoiceCommandAction.OneEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["one nine"],
			ActionToPerform=VoiceCommandAction.OneNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two zero"],
			ActionToPerform=VoiceCommandAction.TwoZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two one"],
			ActionToPerform=VoiceCommandAction.TwoOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two two"],
			ActionToPerform=VoiceCommandAction.TwoTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two three"],
			ActionToPerform=VoiceCommandAction.TwoThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two four"],
			ActionToPerform=VoiceCommandAction.TwoFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two five"],
			ActionToPerform=VoiceCommandAction.TwoFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two six"],
			ActionToPerform=VoiceCommandAction.TwoSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two seven"],
			ActionToPerform=VoiceCommandAction.TwoSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two eight"],
			ActionToPerform=VoiceCommandAction.TwoEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["two nine"],
			ActionToPerform=VoiceCommandAction.TwoNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three zero"],
			ActionToPerform=VoiceCommandAction.ThreeZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three one"],
			ActionToPerform=VoiceCommandAction.ThreeOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three two"],
			ActionToPerform=VoiceCommandAction.ThreeTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three three"],
			ActionToPerform=VoiceCommandAction.ThreeThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three four"],
			ActionToPerform=VoiceCommandAction.ThreeFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three five"],
			ActionToPerform=VoiceCommandAction.ThreeFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three six"],
			ActionToPerform=VoiceCommandAction.ThreeSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three seven"],
			ActionToPerform=VoiceCommandAction.ThreeSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three eight"],
			ActionToPerform=VoiceCommandAction.ThreeEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["three nine"],
			ActionToPerform=VoiceCommandAction.ThreeNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four zero"],
			ActionToPerform=VoiceCommandAction.FourZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four one"],
			ActionToPerform=VoiceCommandAction.FourOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four two"],
			ActionToPerform=VoiceCommandAction.FourTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four three"],
			ActionToPerform=VoiceCommandAction.FourThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four four"],
			ActionToPerform=VoiceCommandAction.FourFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four five"],
			ActionToPerform=VoiceCommandAction.FourFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four six"],
			ActionToPerform=VoiceCommandAction.FourSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four seven"],
			ActionToPerform=VoiceCommandAction.FourSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four eight"],
			ActionToPerform=VoiceCommandAction.FourEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["four nine"],
			ActionToPerform=VoiceCommandAction.FourNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five zero"],
			ActionToPerform=VoiceCommandAction.FiveZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five one"],
			ActionToPerform=VoiceCommandAction.FiveOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five two"],
			ActionToPerform=VoiceCommandAction.FiveTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five three"],
			ActionToPerform=VoiceCommandAction.FiveThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five four"],
			ActionToPerform=VoiceCommandAction.FiveFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five five"],
			ActionToPerform=VoiceCommandAction.FiveFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five six"],
			ActionToPerform=VoiceCommandAction.FiveSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five seven"],
			ActionToPerform=VoiceCommandAction.FiveSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five eight"],
			ActionToPerform=VoiceCommandAction.FiveEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["five nine"],
			ActionToPerform=VoiceCommandAction.FiveNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six zero"],
			ActionToPerform=VoiceCommandAction.SixZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six one"],
			ActionToPerform=VoiceCommandAction.SixOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six two"],
			ActionToPerform=VoiceCommandAction.SixTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six three"],
			ActionToPerform=VoiceCommandAction.SixThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six four"],
			ActionToPerform=VoiceCommandAction.SixFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six five"],
			ActionToPerform=VoiceCommandAction.SixFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six six"],
			ActionToPerform=VoiceCommandAction.SixSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six seven"],
			ActionToPerform=VoiceCommandAction.SixSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six eight"],
			ActionToPerform=VoiceCommandAction.SixEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["six nine"],
			ActionToPerform=VoiceCommandAction.SixNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven zero"],
			ActionToPerform=VoiceCommandAction.SevenZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven one"],
			ActionToPerform=VoiceCommandAction.SevenOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven two"],
			ActionToPerform=VoiceCommandAction.SevenTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven three"],
			ActionToPerform=VoiceCommandAction.SevenThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven four"],
			ActionToPerform=VoiceCommandAction.SevenFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven five"],
			ActionToPerform=VoiceCommandAction.SevenFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven six"],
			ActionToPerform=VoiceCommandAction.SevenSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven seven"],
			ActionToPerform=VoiceCommandAction.SevenSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven eight"],
			ActionToPerform=VoiceCommandAction.SevenEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["seven nine"],
			ActionToPerform=VoiceCommandAction.SevenNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight zero"],
			ActionToPerform=VoiceCommandAction.EightZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight one"],
			ActionToPerform=VoiceCommandAction.EightOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight two"],
			ActionToPerform=VoiceCommandAction.EightTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight three"],
			ActionToPerform=VoiceCommandAction.EightThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight four"],
			ActionToPerform=VoiceCommandAction.EightFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight five"],
			ActionToPerform=VoiceCommandAction.EightFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight six"],
			ActionToPerform=VoiceCommandAction.EightSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight seven"],
			ActionToPerform=VoiceCommandAction.EightSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight eight"],
			ActionToPerform=VoiceCommandAction.EightEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["eight nine"],
			ActionToPerform=VoiceCommandAction.EightNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine zero"],
			ActionToPerform=VoiceCommandAction.NineZero,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine one"],
			ActionToPerform=VoiceCommandAction.NineOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine two"],
			ActionToPerform=VoiceCommandAction.NineTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine three"],
			ActionToPerform=VoiceCommandAction.NineThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine four"],
			ActionToPerform=VoiceCommandAction.NineFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine five"],
			ActionToPerform=VoiceCommandAction.NineFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine six"],
			ActionToPerform=VoiceCommandAction.NineSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine seven"],
			ActionToPerform=VoiceCommandAction.NineSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine eight"],
			ActionToPerform=VoiceCommandAction.NineEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["nine nine"],
			ActionToPerform=VoiceCommandAction.NineNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		#endregion Hard-Coded Triplets and Doubles
		new VoiceCommand {
			Commands=
			[
				"triplet",
				"triplets"
			],
			ActionToPerform=VoiceCommandAction.Triplets,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"check triplets"
			],
			ActionToPerform=VoiceCommandAction.CheckTriplets,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"uncheck triplets"
			],
			ActionToPerform=VoiceCommandAction.UncheckTriplets,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"bleeding",
				"mark bleeding"
			],
			ActionToPerform=VoiceCommandAction.Bleeding,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"bleeding distal",
				"mark bleeding distal"
			],
			ActionToPerform=VoiceCommandAction.BleedingDistal,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"bleeding facial",
				"mark bleeding facial"
			],
			ActionToPerform=VoiceCommandAction.BleedingFacial,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"bleeding lingual",
				"mark bleeding lingual"
			],
			ActionToPerform=VoiceCommandAction.BleedingLingual,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"bleeding mesial",
				"mark bleeding mesial"
			],
			ActionToPerform=VoiceCommandAction.BleedingMesial,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"calculus",
				"mark calculus"
			],
			ActionToPerform=VoiceCommandAction.Calculus,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"calculus distal",
				"mark calculus distal"
			],
			ActionToPerform=VoiceCommandAction.CalculusDistal,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"calculus facial",
				"mark calculus facial"
			],
			ActionToPerform=VoiceCommandAction.CalculusFacial,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"calculus lingual",
				"mark calculus lingual"
			],
			ActionToPerform=VoiceCommandAction.CalculusLingual,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"calculus mesial",
				"mark calculus mesial"
			],
			ActionToPerform=VoiceCommandAction.CalculusMesial,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["plaque"],
			ActionToPerform=VoiceCommandAction.Plaque,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"plaque distal",
				"mark plaque distal"
			],
			ActionToPerform=VoiceCommandAction.PlaqueDistal,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"plaque facial",
				"mark plaque facial"
			],
			ActionToPerform=VoiceCommandAction.PlaqueFacial,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"plaque lingual",
				"mark plaque lingual"
			],
			ActionToPerform=VoiceCommandAction.PlaqueLingual,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"plaque mesial",
				"mark plaque mesial"
			],
			ActionToPerform=VoiceCommandAction.PlaqueMesial,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["suppuration"],
			ActionToPerform=VoiceCommandAction.Suppuration,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"suppuration distal",
				"mark suppuration distal"
			],
			ActionToPerform=VoiceCommandAction.SuppurationDistal,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"suppuration facial",
				"mark suppuration facial"
			],
			ActionToPerform=VoiceCommandAction.SuppurationFacial,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"suppuration lingual",
				"mark suppuration lingual"
			],
			ActionToPerform=VoiceCommandAction.SuppurationLingual,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"suppuration mesial",
				"mark suppuration mesial"
			],
			ActionToPerform=VoiceCommandAction.SuppurationMesial,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["backspace"],
			ActionToPerform=VoiceCommandAction.Backspace,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["left"],
			ActionToPerform=VoiceCommandAction.Left,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["right"],
			ActionToPerform=VoiceCommandAction.Right,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["delete"],
			ActionToPerform=VoiceCommandAction.Delete,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"copy previous",
				"copy previous exam"
			],
			ActionToPerform=VoiceCommandAction.CopyPrevious,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Copying previous exam"
		},
		#region Go To Tooth W/O Facial/Lingual
		GoToToothCommandHelper("one",VoiceCommandAction.GoToToothOne),
		GoToToothCommandHelper("two",VoiceCommandAction.GoToToothTwo),
		GoToToothCommandHelper("three",VoiceCommandAction.GoToToothThree),
		GoToToothCommandHelper("four",VoiceCommandAction.GoToToothFour),
		GoToToothCommandHelper("five",VoiceCommandAction.GoToToothFive),
		GoToToothCommandHelper("six",VoiceCommandAction.GoToToothSix),
		GoToToothCommandHelper("seven",VoiceCommandAction.GoToToothSeven),
		GoToToothCommandHelper("eight",VoiceCommandAction.GoToToothEight),
		GoToToothCommandHelper("nine",VoiceCommandAction.GoToToothNine),
		GoToToothCommandHelper("ten",VoiceCommandAction.GoToToothTen),
		GoToToothCommandHelper("eleven",VoiceCommandAction.GoToToothEleven),
		GoToToothCommandHelper("twelve",VoiceCommandAction.GoToToothTwelve),
		GoToToothCommandHelper("thirteen",VoiceCommandAction.GoToToothThirteen),
		GoToToothCommandHelper("fourteen",VoiceCommandAction.GoToToothFourteen),
		GoToToothCommandHelper("fifteen",VoiceCommandAction.GoToToothFifteen),
		GoToToothCommandHelper("sixteen",VoiceCommandAction.GoToToothSixteen),
		GoToToothCommandHelper("seventeen",VoiceCommandAction.GoToToothSeventeen),
		GoToToothCommandHelper("eighteen",VoiceCommandAction.GoToToothEighteen),
		GoToToothCommandHelper("nineteen",VoiceCommandAction.GoToToothNineteen),
		GoToToothCommandHelper("twenty",VoiceCommandAction.GoToToothTwenty),
		GoToToothCommandHelper("twenty one",VoiceCommandAction.GoToToothTwentyOne),
		GoToToothCommandHelper("twenty two",VoiceCommandAction.GoToToothTwentyTwo),
		GoToToothCommandHelper("twenty three",VoiceCommandAction.GoToToothTwentyThree),
		GoToToothCommandHelper("twenty four",VoiceCommandAction.GoToToothTwentyFour),
		GoToToothCommandHelper("twenty five",VoiceCommandAction.GoToToothTwentyFive),
		GoToToothCommandHelper("twenty six",VoiceCommandAction.GoToToothTwentySix),
		GoToToothCommandHelper("twenty seven",VoiceCommandAction.GoToToothTwentySeven),
		GoToToothCommandHelper("twenty eight",VoiceCommandAction.GoToToothTwentyEight),
		GoToToothCommandHelper("twenty nine",VoiceCommandAction.GoToToothTwentyNine),
		GoToToothCommandHelper("thirty",VoiceCommandAction.GoToToothThirty),
		GoToToothCommandHelper("thirty one",VoiceCommandAction.GoToToothThirtyOne),
		GoToToothCommandHelper("thirty two",VoiceCommandAction.GoToToothThirtyTwo),
		#endregion
		#region Go To Tooth W/ Surfaces
		GoToToothWithSurfaceXCommandHelper("one","facial",VoiceCommandAction.GoToToothOneFacial),
		GoToToothWithSurfaceXCommandHelper("two","facial",VoiceCommandAction.GoToToothTwoFacial),
		GoToToothWithSurfaceXCommandHelper("three","facial",VoiceCommandAction.GoToToothThreeFacial),
		GoToToothWithSurfaceXCommandHelper("four","facial",VoiceCommandAction.GoToToothFourFacial),
		GoToToothWithSurfaceXCommandHelper("five","facial",VoiceCommandAction.GoToToothFiveFacial),
		GoToToothWithSurfaceXCommandHelper("six","facial",VoiceCommandAction.GoToToothSixFacial),
		GoToToothWithSurfaceXCommandHelper("seven","facial",VoiceCommandAction.GoToToothSevenFacial),
		GoToToothWithSurfaceXCommandHelper("eight","facial",VoiceCommandAction.GoToToothEightFacial),
		GoToToothWithSurfaceXCommandHelper("nine","facial",VoiceCommandAction.GoToToothNineFacial),
		GoToToothWithSurfaceXCommandHelper("ten","facial",VoiceCommandAction.GoToToothTenFacial),
		GoToToothWithSurfaceXCommandHelper("eleven","facial",VoiceCommandAction.GoToToothElevenFacial),
		GoToToothWithSurfaceXCommandHelper("twelve","facial",VoiceCommandAction.GoToToothTwelveFacial),
		GoToToothWithSurfaceXCommandHelper("thirteen","facial",VoiceCommandAction.GoToToothThirteenFacial),
		GoToToothWithSurfaceXCommandHelper("fourteen","facial",VoiceCommandAction.GoToToothFourteenFacial),
		GoToToothWithSurfaceXCommandHelper("fifteen","facial",VoiceCommandAction.GoToToothFifteenFacial),
		GoToToothWithSurfaceXCommandHelper("sixteen","facial",VoiceCommandAction.GoToToothSixteenFacial),
		GoToToothWithSurfaceXCommandHelper("seventeen","facial",VoiceCommandAction.GoToToothSeventeenFacial),
		GoToToothWithSurfaceXCommandHelper("eighteen","facial",VoiceCommandAction.GoToToothEighteenFacial),
		GoToToothWithSurfaceXCommandHelper("nineteen","facial",VoiceCommandAction.GoToToothNineteenFacial),
		GoToToothWithSurfaceXCommandHelper("twenty","facial",VoiceCommandAction.GoToToothTwentyFacial),
		GoToToothWithSurfaceXCommandHelper("twenty one","facial",VoiceCommandAction.GoToToothTwentyOneFacial),
		GoToToothWithSurfaceXCommandHelper("twenty two","facial",VoiceCommandAction.GoToToothTwentyTwoFacial),
		GoToToothWithSurfaceXCommandHelper("twenty three","facial",VoiceCommandAction.GoToToothTwentyThreeFacial),
		GoToToothWithSurfaceXCommandHelper("twenty four","facial",VoiceCommandAction.GoToToothTwentyFourFacial),
		GoToToothWithSurfaceXCommandHelper("twenty five","facial",VoiceCommandAction.GoToToothTwentyFiveFacial),
		GoToToothWithSurfaceXCommandHelper("twenty six","facial",VoiceCommandAction.GoToToothTwentySixFacial),
		GoToToothWithSurfaceXCommandHelper("twenty seven","facial",VoiceCommandAction.GoToToothTwentySevenFacial),
		GoToToothWithSurfaceXCommandHelper("twenty eight","facial",VoiceCommandAction.GoToToothTwentyEightFacial),
		GoToToothWithSurfaceXCommandHelper("twenty nine","facial",VoiceCommandAction.GoToToothTwentyNineFacial),
		GoToToothWithSurfaceXCommandHelper("thirty","facial",VoiceCommandAction.GoToToothThirtyFacial),
		GoToToothWithSurfaceXCommandHelper("thirty one","facial",VoiceCommandAction.GoToToothThirtyOneFacial),
		GoToToothWithSurfaceXCommandHelper("thirty two","facial",VoiceCommandAction.GoToToothThirtyTwoFacial),
		GoToToothWithSurfaceXCommandHelper("one","lingual",VoiceCommandAction.GoToToothOneLingual),
		GoToToothWithSurfaceXCommandHelper("two","lingual",VoiceCommandAction.GoToToothTwoLingual),
		GoToToothWithSurfaceXCommandHelper("three","lingual",VoiceCommandAction.GoToToothThreeLingual),
		GoToToothWithSurfaceXCommandHelper("four","lingual",VoiceCommandAction.GoToToothFourLingual),
		GoToToothWithSurfaceXCommandHelper("five","lingual",VoiceCommandAction.GoToToothFiveLingual),
		GoToToothWithSurfaceXCommandHelper("six","lingual",VoiceCommandAction.GoToToothSixLingual),
		GoToToothWithSurfaceXCommandHelper("seven","lingual",VoiceCommandAction.GoToToothSevenLingual),
		GoToToothWithSurfaceXCommandHelper("eight","lingual",VoiceCommandAction.GoToToothEightLingual),
		GoToToothWithSurfaceXCommandHelper("nine","lingual",VoiceCommandAction.GoToToothNineLingual),
		GoToToothWithSurfaceXCommandHelper("ten","lingual",VoiceCommandAction.GoToToothTenLingual),
		GoToToothWithSurfaceXCommandHelper("eleven","lingual",VoiceCommandAction.GoToToothElevenLingual),
		GoToToothWithSurfaceXCommandHelper("twelve","lingual",VoiceCommandAction.GoToToothTwelveLingual),
		GoToToothWithSurfaceXCommandHelper("thirteen","lingual",VoiceCommandAction.GoToToothThirteenLingual),
		GoToToothWithSurfaceXCommandHelper("fourteen","lingual",VoiceCommandAction.GoToToothFourteenLingual),
		GoToToothWithSurfaceXCommandHelper("fifteen","lingual",VoiceCommandAction.GoToToothFifteenLingual),
		GoToToothWithSurfaceXCommandHelper("sixteen","lingual",VoiceCommandAction.GoToToothSixteenLingual),
		GoToToothWithSurfaceXCommandHelper("seventeen","lingual",VoiceCommandAction.GoToToothSeventeenLingual),
		GoToToothWithSurfaceXCommandHelper("eighteen","lingual",VoiceCommandAction.GoToToothEighteenLingual),
		GoToToothWithSurfaceXCommandHelper("nineteen","lingual",VoiceCommandAction.GoToToothNineteenLingual),
		GoToToothWithSurfaceXCommandHelper("twenty","lingual",VoiceCommandAction.GoToToothTwentyLingual),
		GoToToothWithSurfaceXCommandHelper("twenty one","lingual",VoiceCommandAction.GoToToothTwentyOneLingual),
		GoToToothWithSurfaceXCommandHelper("twenty two","lingual",VoiceCommandAction.GoToToothTwentyTwoLingual),
		GoToToothWithSurfaceXCommandHelper("twenty three","lingual",VoiceCommandAction.GoToToothTwentyThreeLingual),
		GoToToothWithSurfaceXCommandHelper("twenty four","lingual",VoiceCommandAction.GoToToothTwentyFourLingual),
		GoToToothWithSurfaceXCommandHelper("twenty five","lingual",VoiceCommandAction.GoToToothTwentyFiveLingual),
		GoToToothWithSurfaceXCommandHelper("twenty six","lingual",VoiceCommandAction.GoToToothTwentySixLingual),
		GoToToothWithSurfaceXCommandHelper("twenty seven","lingual",VoiceCommandAction.GoToToothTwentySevenLingual),
		GoToToothWithSurfaceXCommandHelper("twenty eight","lingual",VoiceCommandAction.GoToToothTwentyEightLingual),
		GoToToothWithSurfaceXCommandHelper("twenty nine","lingual",VoiceCommandAction.GoToToothTwentyNineLingual),
		GoToToothWithSurfaceXCommandHelper("thirty","lingual",VoiceCommandAction.GoToToothThirtyLingual),
		GoToToothWithSurfaceXCommandHelper("thirty one","lingual",VoiceCommandAction.GoToToothThirtyOneLingual),
		GoToToothWithSurfaceXCommandHelper("thirty two","lingual",VoiceCommandAction.GoToToothThirtyTwoLingual),

		GoToToothWithSurfaceXCommandHelper("one","distal",VoiceCommandAction.GoToToothOneDistal),
		GoToToothWithSurfaceXCommandHelper("two","distal",VoiceCommandAction.GoToToothTwoDistal),
		GoToToothWithSurfaceXCommandHelper("three","distal",VoiceCommandAction.GoToToothThreeDistal),
		GoToToothWithSurfaceXCommandHelper("four","distal",VoiceCommandAction.GoToToothFourDistal),
		GoToToothWithSurfaceXCommandHelper("five","distal",VoiceCommandAction.GoToToothFiveDistal),
		GoToToothWithSurfaceXCommandHelper("six","distal",VoiceCommandAction.GoToToothSixDistal),
		GoToToothWithSurfaceXCommandHelper("seven","distal",VoiceCommandAction.GoToToothSevenDistal),
		GoToToothWithSurfaceXCommandHelper("eight","distal",VoiceCommandAction.GoToToothEightDistal),
		GoToToothWithSurfaceXCommandHelper("nine","distal",VoiceCommandAction.GoToToothNineDistal),
		GoToToothWithSurfaceXCommandHelper("ten","distal",VoiceCommandAction.GoToToothTenDistal),
		GoToToothWithSurfaceXCommandHelper("eleven","distal",VoiceCommandAction.GoToToothElevenDistal),
		GoToToothWithSurfaceXCommandHelper("twelve","distal",VoiceCommandAction.GoToToothTwelveDistal),
		GoToToothWithSurfaceXCommandHelper("thirteen","distal",VoiceCommandAction.GoToToothThirteenDistal),
		GoToToothWithSurfaceXCommandHelper("fourteen","distal",VoiceCommandAction.GoToToothFourteenDistal),
		GoToToothWithSurfaceXCommandHelper("fifteen","distal",VoiceCommandAction.GoToToothFifteenDistal),
		GoToToothWithSurfaceXCommandHelper("sixteen","distal",VoiceCommandAction.GoToToothSixteenDistal),
		GoToToothWithSurfaceXCommandHelper("seventeen","distal",VoiceCommandAction.GoToToothSeventeenDistal),
		GoToToothWithSurfaceXCommandHelper("eighteen","distal",VoiceCommandAction.GoToToothEighteenDistal),
		GoToToothWithSurfaceXCommandHelper("nineteen","distal",VoiceCommandAction.GoToToothNineteenDistal),
		GoToToothWithSurfaceXCommandHelper("twenty","distal",VoiceCommandAction.GoToToothTwentyDistal),
		GoToToothWithSurfaceXCommandHelper("twenty one","distal",VoiceCommandAction.GoToToothTwentyOneDistal),
		GoToToothWithSurfaceXCommandHelper("twenty two","distal",VoiceCommandAction.GoToToothTwentyTwoDistal),
		GoToToothWithSurfaceXCommandHelper("twenty three","distal",VoiceCommandAction.GoToToothTwentyThreeDistal),
		GoToToothWithSurfaceXCommandHelper("twenty four","distal",VoiceCommandAction.GoToToothTwentyFourDistal),
		GoToToothWithSurfaceXCommandHelper("twenty five","distal",VoiceCommandAction.GoToToothTwentyFiveDistal),
		GoToToothWithSurfaceXCommandHelper("twenty six","distal",VoiceCommandAction.GoToToothTwentySixDistal),
		GoToToothWithSurfaceXCommandHelper("twenty seven","distal",VoiceCommandAction.GoToToothTwentySevenDistal),
		GoToToothWithSurfaceXCommandHelper("twenty eight","distal",VoiceCommandAction.GoToToothTwentyEightDistal),
		GoToToothWithSurfaceXCommandHelper("twenty nine","distal",VoiceCommandAction.GoToToothTwentyNineDistal),
		GoToToothWithSurfaceXCommandHelper("thirty","distal",VoiceCommandAction.GoToToothThirtyDistal),
		GoToToothWithSurfaceXCommandHelper("thirty one","distal",VoiceCommandAction.GoToToothThirtyOneDistal),
		GoToToothWithSurfaceXCommandHelper("thirty two","distal",VoiceCommandAction.GoToToothThirtyTwoDistal),

		GoToToothWithSurfaceCommandHelper("one","distal",true,VoiceCommandAction.GoToToothOneDistalFacial),
		GoToToothWithSurfaceCommandHelper("two","distal",true,VoiceCommandAction.GoToToothTwoDistalFacial),
		GoToToothWithSurfaceCommandHelper("three","distal",true,VoiceCommandAction.GoToToothThreeDistalFacial),
		GoToToothWithSurfaceCommandHelper("four","distal",true,VoiceCommandAction.GoToToothFourDistalFacial),
		GoToToothWithSurfaceCommandHelper("five","distal",true,VoiceCommandAction.GoToToothFiveDistalFacial),
		GoToToothWithSurfaceCommandHelper("six","distal",true,VoiceCommandAction.GoToToothSixDistalFacial),
		GoToToothWithSurfaceCommandHelper("seven","distal",true,VoiceCommandAction.GoToToothSevenDistalFacial),
		GoToToothWithSurfaceCommandHelper("eight","distal",true,VoiceCommandAction.GoToToothEightDistalFacial),
		GoToToothWithSurfaceCommandHelper("nine","distal",true,VoiceCommandAction.GoToToothNineDistalFacial),
		GoToToothWithSurfaceCommandHelper("ten","distal",true,VoiceCommandAction.GoToToothTenDistalFacial),
		GoToToothWithSurfaceCommandHelper("eleven","distal",true,VoiceCommandAction.GoToToothElevenDistalFacial),
		GoToToothWithSurfaceCommandHelper("twelve","distal",true,VoiceCommandAction.GoToToothTwelveDistalFacial),
		GoToToothWithSurfaceCommandHelper("thirteen","distal",true,VoiceCommandAction.GoToToothThirteenDistalFacial),
		GoToToothWithSurfaceCommandHelper("fourteen","distal",true,VoiceCommandAction.GoToToothFourteenDistalFacial),
		GoToToothWithSurfaceCommandHelper("fifteen","distal",true,VoiceCommandAction.GoToToothFifteenDistalFacial),
		GoToToothWithSurfaceCommandHelper("sixteen","distal",true,VoiceCommandAction.GoToToothSixteenDistalFacial),
		GoToToothWithSurfaceCommandHelper("seventeen","distal",true,VoiceCommandAction.GoToToothSeventeenDistalFacial),
		GoToToothWithSurfaceCommandHelper("eighteen","distal",true,VoiceCommandAction.GoToToothEighteenDistalFacial),
		GoToToothWithSurfaceCommandHelper("nineteen","distal",true,VoiceCommandAction.GoToToothNineteenDistalFacial),
		GoToToothWithSurfaceCommandHelper("twenty","distal",true,VoiceCommandAction.GoToToothTwentyDistalFacial),
		GoToToothWithSurfaceCommandHelper("twenty one","distal",true,VoiceCommandAction.GoToToothTwentyOneDistalFacial),
		GoToToothWithSurfaceCommandHelper("twenty two","distal",true,VoiceCommandAction.GoToToothTwentyTwoDistalFacial),
		GoToToothWithSurfaceCommandHelper("twenty three","distal",true,VoiceCommandAction.GoToToothTwentyThreeDistalFacial),
		GoToToothWithSurfaceCommandHelper("twenty four","distal",true,VoiceCommandAction.GoToToothTwentyFourDistalFacial),
		GoToToothWithSurfaceCommandHelper("twenty five","distal",true,VoiceCommandAction.GoToToothTwentyFiveDistalFacial),
		GoToToothWithSurfaceCommandHelper("twenty six","distal",true,VoiceCommandAction.GoToToothTwentySixDistalFacial),
		GoToToothWithSurfaceCommandHelper("twenty seven","distal",true,VoiceCommandAction.GoToToothTwentySevenDistalFacial),
		GoToToothWithSurfaceCommandHelper("twenty eight","distal",true,VoiceCommandAction.GoToToothTwentyEightDistalFacial),
		GoToToothWithSurfaceCommandHelper("twenty nine","distal",true,VoiceCommandAction.GoToToothTwentyNineDistalFacial),
		GoToToothWithSurfaceCommandHelper("thirty","distal",true,VoiceCommandAction.GoToToothThirtyDistalFacial),
		GoToToothWithSurfaceCommandHelper("thirty one","distal",true,VoiceCommandAction.GoToToothThirtyOneDistalFacial),
		GoToToothWithSurfaceCommandHelper("thirty two","distal",true,VoiceCommandAction.GoToToothThirtyTwoDistalFacial),
		GoToToothWithSurfaceCommandHelper("one","distal",false,VoiceCommandAction.GoToToothOneDistalLingual),
		GoToToothWithSurfaceCommandHelper("two","distal",false,VoiceCommandAction.GoToToothTwoDistalLingual),
		GoToToothWithSurfaceCommandHelper("three","distal",false,VoiceCommandAction.GoToToothThreeDistalLingual),
		GoToToothWithSurfaceCommandHelper("four","distal",false,VoiceCommandAction.GoToToothFourDistalLingual),
		GoToToothWithSurfaceCommandHelper("five","distal",false,VoiceCommandAction.GoToToothFiveDistalLingual),
		GoToToothWithSurfaceCommandHelper("six","distal",false,VoiceCommandAction.GoToToothSixDistalLingual),
		GoToToothWithSurfaceCommandHelper("seven","distal",false,VoiceCommandAction.GoToToothSevenDistalLingual),
		GoToToothWithSurfaceCommandHelper("eight","distal",false,VoiceCommandAction.GoToToothEightDistalLingual),
		GoToToothWithSurfaceCommandHelper("nine","distal",false,VoiceCommandAction.GoToToothNineDistalLingual),
		GoToToothWithSurfaceCommandHelper("ten","distal",false,VoiceCommandAction.GoToToothTenDistalLingual),
		GoToToothWithSurfaceCommandHelper("eleven","distal",false,VoiceCommandAction.GoToToothElevenDistalLingual),
		GoToToothWithSurfaceCommandHelper("twelve","distal",false,VoiceCommandAction.GoToToothTwelveDistalLingual),
		GoToToothWithSurfaceCommandHelper("thirteen","distal",false,VoiceCommandAction.GoToToothThirteenDistalLingual),
		GoToToothWithSurfaceCommandHelper("fourteen","distal",false,VoiceCommandAction.GoToToothFourteenDistalLingual),
		GoToToothWithSurfaceCommandHelper("fifteen","distal",false,VoiceCommandAction.GoToToothFifteenDistalLingual),
		GoToToothWithSurfaceCommandHelper("sixteen","distal",false,VoiceCommandAction.GoToToothSixteenDistalLingual),
		GoToToothWithSurfaceCommandHelper("seventeen","distal",false,VoiceCommandAction.GoToToothSeventeenDistalLingual),
		GoToToothWithSurfaceCommandHelper("eighteen","distal",false,VoiceCommandAction.GoToToothEighteenDistalLingual),
		GoToToothWithSurfaceCommandHelper("nineteen","distal",false,VoiceCommandAction.GoToToothNineteenDistalLingual),
		GoToToothWithSurfaceCommandHelper("twenty","distal",false,VoiceCommandAction.GoToToothTwentyDistalLingual),
		GoToToothWithSurfaceCommandHelper("twenty one","distal",false,VoiceCommandAction.GoToToothTwentyOneDistalLingual),
		GoToToothWithSurfaceCommandHelper("twenty two","distal",false,VoiceCommandAction.GoToToothTwentyTwoDistalLingual),
		GoToToothWithSurfaceCommandHelper("twenty three","distal",false,VoiceCommandAction.GoToToothTwentyThreeDistalLingual),
		GoToToothWithSurfaceCommandHelper("twenty four","distal",false,VoiceCommandAction.GoToToothTwentyFourDistalLingual),
		GoToToothWithSurfaceCommandHelper("twenty five","distal",false,VoiceCommandAction.GoToToothTwentyFiveDistalLingual),
		GoToToothWithSurfaceCommandHelper("twenty six","distal",false,VoiceCommandAction.GoToToothTwentySixDistalLingual),
		GoToToothWithSurfaceCommandHelper("twenty seven","distal",false,VoiceCommandAction.GoToToothTwentySevenDistalLingual),
		GoToToothWithSurfaceCommandHelper("twenty eight","distal",false,VoiceCommandAction.GoToToothTwentyEightDistalLingual),
		GoToToothWithSurfaceCommandHelper("twenty nine","distal",false,VoiceCommandAction.GoToToothTwentyNineDistalLingual),
		GoToToothWithSurfaceCommandHelper("thirty","distal",false,VoiceCommandAction.GoToToothThirtyDistalLingual),
		GoToToothWithSurfaceCommandHelper("thirty one","distal",false,VoiceCommandAction.GoToToothThirtyOneDistalLingual),
		GoToToothWithSurfaceCommandHelper("thirty two","distal",false,VoiceCommandAction.GoToToothThirtyTwoDistalLingual),

		GoToToothWithSurfaceXCommandHelper("one","mesial",VoiceCommandAction.GoToToothOneMesial),
		GoToToothWithSurfaceXCommandHelper("two","mesial",VoiceCommandAction.GoToToothTwoMesial),
		GoToToothWithSurfaceXCommandHelper("three","mesial",VoiceCommandAction.GoToToothThreeMesial),
		GoToToothWithSurfaceXCommandHelper("four","mesial",VoiceCommandAction.GoToToothFourMesial),
		GoToToothWithSurfaceXCommandHelper("five","mesial",VoiceCommandAction.GoToToothFiveMesial),
		GoToToothWithSurfaceXCommandHelper("six","mesial",VoiceCommandAction.GoToToothSixMesial),
		GoToToothWithSurfaceXCommandHelper("seven","mesial",VoiceCommandAction.GoToToothSevenMesial),
		GoToToothWithSurfaceXCommandHelper("eight","mesial",VoiceCommandAction.GoToToothEightMesial),
		GoToToothWithSurfaceXCommandHelper("nine","mesial",VoiceCommandAction.GoToToothNineMesial),
		GoToToothWithSurfaceXCommandHelper("ten","mesial",VoiceCommandAction.GoToToothTenMesial),
		GoToToothWithSurfaceXCommandHelper("eleven","mesial",VoiceCommandAction.GoToToothElevenMesial),
		GoToToothWithSurfaceXCommandHelper("twelve","mesial",VoiceCommandAction.GoToToothTwelveMesial),
		GoToToothWithSurfaceXCommandHelper("thirteen","mesial",VoiceCommandAction.GoToToothThirteenMesial),
		GoToToothWithSurfaceXCommandHelper("fourteen","mesial",VoiceCommandAction.GoToToothFourteenMesial),
		GoToToothWithSurfaceXCommandHelper("fifteen","mesial",VoiceCommandAction.GoToToothFifteenMesial),
		GoToToothWithSurfaceXCommandHelper("sixteen","mesial",VoiceCommandAction.GoToToothSixteenMesial),
		GoToToothWithSurfaceXCommandHelper("seventeen","mesial",VoiceCommandAction.GoToToothSeventeenMesial),
		GoToToothWithSurfaceXCommandHelper("eighteen","mesial",VoiceCommandAction.GoToToothEighteenMesial),
		GoToToothWithSurfaceXCommandHelper("nineteen","mesial",VoiceCommandAction.GoToToothNineteenMesial),
		GoToToothWithSurfaceXCommandHelper("twenty","mesial",VoiceCommandAction.GoToToothTwentyMesial),
		GoToToothWithSurfaceXCommandHelper("twenty one","mesial",VoiceCommandAction.GoToToothTwentyOneMesial),
		GoToToothWithSurfaceXCommandHelper("twenty two","mesial",VoiceCommandAction.GoToToothTwentyTwoMesial),
		GoToToothWithSurfaceXCommandHelper("twenty three","mesial",VoiceCommandAction.GoToToothTwentyThreeMesial),
		GoToToothWithSurfaceXCommandHelper("twenty four","mesial",VoiceCommandAction.GoToToothTwentyFourMesial),
		GoToToothWithSurfaceXCommandHelper("twenty five","mesial",VoiceCommandAction.GoToToothTwentyFiveMesial),
		GoToToothWithSurfaceXCommandHelper("twenty six","mesial",VoiceCommandAction.GoToToothTwentySixMesial),
		GoToToothWithSurfaceXCommandHelper("twenty seven","mesial",VoiceCommandAction.GoToToothTwentySevenMesial),
		GoToToothWithSurfaceXCommandHelper("twenty eight","mesial",VoiceCommandAction.GoToToothTwentyEightMesial),
		GoToToothWithSurfaceXCommandHelper("twenty nine","mesial",VoiceCommandAction.GoToToothTwentyNineMesial),
		GoToToothWithSurfaceXCommandHelper("thirty","mesial",VoiceCommandAction.GoToToothThirtyMesial),
		GoToToothWithSurfaceXCommandHelper("thirty one","mesial",VoiceCommandAction.GoToToothThirtyOneMesial),
		GoToToothWithSurfaceXCommandHelper("thirty two","mesial",VoiceCommandAction.GoToToothThirtyTwoMesial),

		GoToToothWithSurfaceCommandHelper("one","mesial",true,VoiceCommandAction.GoToToothOneMesialFacial),
		GoToToothWithSurfaceCommandHelper("two","mesial",true,VoiceCommandAction.GoToToothTwoMesialFacial),
		GoToToothWithSurfaceCommandHelper("three","mesial",true,VoiceCommandAction.GoToToothThreeMesialFacial),
		GoToToothWithSurfaceCommandHelper("four","mesial",true,VoiceCommandAction.GoToToothFourMesialFacial),
		GoToToothWithSurfaceCommandHelper("five","mesial",true,VoiceCommandAction.GoToToothFiveMesialFacial),
		GoToToothWithSurfaceCommandHelper("six","mesial",true,VoiceCommandAction.GoToToothSixMesialFacial),
		GoToToothWithSurfaceCommandHelper("seven","mesial",true,VoiceCommandAction.GoToToothSevenMesialFacial),
		GoToToothWithSurfaceCommandHelper("eight","mesial",true,VoiceCommandAction.GoToToothEightMesialFacial),
		GoToToothWithSurfaceCommandHelper("nine","mesial",true,VoiceCommandAction.GoToToothNineMesialFacial),
		GoToToothWithSurfaceCommandHelper("ten","mesial",true,VoiceCommandAction.GoToToothTenMesialFacial),
		GoToToothWithSurfaceCommandHelper("eleven","mesial",true,VoiceCommandAction.GoToToothElevenMesialFacial),
		GoToToothWithSurfaceCommandHelper("twelve","mesial",true,VoiceCommandAction.GoToToothTwelveMesialFacial),
		GoToToothWithSurfaceCommandHelper("thirteen","mesial",true,VoiceCommandAction.GoToToothThirteenMesialFacial),
		GoToToothWithSurfaceCommandHelper("fourteen","mesial",true,VoiceCommandAction.GoToToothFourteenMesialFacial),
		GoToToothWithSurfaceCommandHelper("fifteen","mesial",true,VoiceCommandAction.GoToToothFifteenMesialFacial),
		GoToToothWithSurfaceCommandHelper("sixteen","mesial",true,VoiceCommandAction.GoToToothSixteenMesialFacial),
		GoToToothWithSurfaceCommandHelper("seventeen","mesial",true,VoiceCommandAction.GoToToothSeventeenMesialFacial),
		GoToToothWithSurfaceCommandHelper("eighteen","mesial",true,VoiceCommandAction.GoToToothEighteenMesialFacial),
		GoToToothWithSurfaceCommandHelper("nineteen","mesial",true,VoiceCommandAction.GoToToothNineteenMesialFacial),
		GoToToothWithSurfaceCommandHelper("twenty","mesial",true,VoiceCommandAction.GoToToothTwentyMesialFacial),
		GoToToothWithSurfaceCommandHelper("twenty one","mesial",true,VoiceCommandAction.GoToToothTwentyOneMesialFacial),
		GoToToothWithSurfaceCommandHelper("twenty two","mesial",true,VoiceCommandAction.GoToToothTwentyTwoMesialFacial),
		GoToToothWithSurfaceCommandHelper("twenty three","mesial",true,VoiceCommandAction.GoToToothTwentyThreeMesialFacial),
		GoToToothWithSurfaceCommandHelper("twenty four","mesial",true,VoiceCommandAction.GoToToothTwentyFourMesialFacial),
		GoToToothWithSurfaceCommandHelper("twenty five","mesial",true,VoiceCommandAction.GoToToothTwentyFiveMesialFacial),
		GoToToothWithSurfaceCommandHelper("twenty six","mesial",true,VoiceCommandAction.GoToToothTwentySixMesialFacial),
		GoToToothWithSurfaceCommandHelper("twenty seven","mesial",true,VoiceCommandAction.GoToToothTwentySevenMesialFacial),
		GoToToothWithSurfaceCommandHelper("twenty eight","mesial",true,VoiceCommandAction.GoToToothTwentyEightMesialFacial),
		GoToToothWithSurfaceCommandHelper("twenty nine","mesial",true,VoiceCommandAction.GoToToothTwentyNineMesialFacial),
		GoToToothWithSurfaceCommandHelper("thirty","mesial",true,VoiceCommandAction.GoToToothThirtyMesialFacial),
		GoToToothWithSurfaceCommandHelper("thirty one","mesial",true,VoiceCommandAction.GoToToothThirtyOneMesialFacial),
		GoToToothWithSurfaceCommandHelper("thirty two","mesial",true,VoiceCommandAction.GoToToothThirtyTwoMesialFacial),
		GoToToothWithSurfaceCommandHelper("one","mesial",false,VoiceCommandAction.GoToToothOneMesialLingual),
		GoToToothWithSurfaceCommandHelper("two","mesial",false,VoiceCommandAction.GoToToothTwoMesialLingual),
		GoToToothWithSurfaceCommandHelper("three","mesial",false,VoiceCommandAction.GoToToothThreeMesialLingual),
		GoToToothWithSurfaceCommandHelper("four","mesial",false,VoiceCommandAction.GoToToothFourMesialLingual),
		GoToToothWithSurfaceCommandHelper("five","mesial",false,VoiceCommandAction.GoToToothFiveMesialLingual),
		GoToToothWithSurfaceCommandHelper("six","mesial",false,VoiceCommandAction.GoToToothSixMesialLingual),
		GoToToothWithSurfaceCommandHelper("seven","mesial",false,VoiceCommandAction.GoToToothSevenMesialLingual),
		GoToToothWithSurfaceCommandHelper("eight","mesial",false,VoiceCommandAction.GoToToothEightMesialLingual),
		GoToToothWithSurfaceCommandHelper("nine","mesial",false,VoiceCommandAction.GoToToothNineMesialLingual),
		GoToToothWithSurfaceCommandHelper("ten","mesial",false,VoiceCommandAction.GoToToothTenMesialLingual),
		GoToToothWithSurfaceCommandHelper("eleven","mesial",false,VoiceCommandAction.GoToToothElevenMesialLingual),
		GoToToothWithSurfaceCommandHelper("twelve","mesial",false,VoiceCommandAction.GoToToothTwelveMesialLingual),
		GoToToothWithSurfaceCommandHelper("thirteen","mesial",false,VoiceCommandAction.GoToToothThirteenMesialLingual),
		GoToToothWithSurfaceCommandHelper("fourteen","mesial",false,VoiceCommandAction.GoToToothFourteenMesialLingual),
		GoToToothWithSurfaceCommandHelper("fifteen","mesial",false,VoiceCommandAction.GoToToothFifteenMesialLingual),
		GoToToothWithSurfaceCommandHelper("sixteen","mesial",false,VoiceCommandAction.GoToToothSixteenMesialLingual),
		GoToToothWithSurfaceCommandHelper("seventeen","mesial",false,VoiceCommandAction.GoToToothSeventeenMesialLingual),
		GoToToothWithSurfaceCommandHelper("eighteen","mesial",false,VoiceCommandAction.GoToToothEighteenMesialLingual),
		GoToToothWithSurfaceCommandHelper("nineteen","mesial",false,VoiceCommandAction.GoToToothNineteenMesialLingual),
		GoToToothWithSurfaceCommandHelper("twenty","mesial",false,VoiceCommandAction.GoToToothTwentyMesialLingual),
		GoToToothWithSurfaceCommandHelper("twenty one","mesial",false,VoiceCommandAction.GoToToothTwentyOneMesialLingual),
		GoToToothWithSurfaceCommandHelper("twenty two","mesial",false,VoiceCommandAction.GoToToothTwentyTwoMesialLingual),
		GoToToothWithSurfaceCommandHelper("twenty three","mesial",false,VoiceCommandAction.GoToToothTwentyThreeMesialLingual),
		GoToToothWithSurfaceCommandHelper("twenty four","mesial",false,VoiceCommandAction.GoToToothTwentyFourMesialLingual),
		GoToToothWithSurfaceCommandHelper("twenty five","mesial",false,VoiceCommandAction.GoToToothTwentyFiveMesialLingual),
		GoToToothWithSurfaceCommandHelper("twenty six","mesial",false,VoiceCommandAction.GoToToothTwentySixMesialLingual),
		GoToToothWithSurfaceCommandHelper("twenty seven","mesial",false,VoiceCommandAction.GoToToothTwentySevenMesialLingual),
		GoToToothWithSurfaceCommandHelper("twenty eight","mesial",false,VoiceCommandAction.GoToToothTwentyEightMesialLingual),
		GoToToothWithSurfaceCommandHelper("twenty nine","mesial",false,VoiceCommandAction.GoToToothTwentyNineMesialLingual),
		GoToToothWithSurfaceCommandHelper("thirty","mesial",false,VoiceCommandAction.GoToToothThirtyMesialLingual),
		GoToToothWithSurfaceCommandHelper("thirty one","mesial",false,VoiceCommandAction.GoToToothThirtyOneMesialLingual),
		GoToToothWithSurfaceCommandHelper("thirty two","mesial",false,VoiceCommandAction.GoToToothThirtyTwoMesialLingual),
		#endregion
		new VoiceCommand {
			Commands= ["probing"],
			ActionToPerform=VoiceCommandAction.Probing,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands=
			[
				"Muco Gingival Junction",
				"MGJ"
			],
			ActionToPerform=VoiceCommandAction.MucoGingivalJunction,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="MGJ"
		},
		new VoiceCommand {
			Commands= ["Gingival Margin"],
			ActionToPerform=VoiceCommandAction.GingivalMargin,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["Furcation"],
			ActionToPerform=VoiceCommandAction.Furcation,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["mobility"],
			ActionToPerform=VoiceCommandAction.Mobility,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["plus one"],
			ActionToPerform=VoiceCommandAction.PlusOne,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["plus two"],
			ActionToPerform=VoiceCommandAction.PlusTwo,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["plus three"],
			ActionToPerform=VoiceCommandAction.PlusThree,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["plus four"],
			ActionToPerform=VoiceCommandAction.PlusFour,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["plus five"],
			ActionToPerform=VoiceCommandAction.PlusFive,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["plus six"],
			ActionToPerform=VoiceCommandAction.PlusSix,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["plus seven"],
			ActionToPerform=VoiceCommandAction.PlusSeven,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["plus eight"],
			ActionToPerform=VoiceCommandAction.PlusEight,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["plus nine"],
			ActionToPerform=VoiceCommandAction.PlusNine,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		new VoiceCommand {
			Commands= ["skip tooth one"],
			ActionToPerform=VoiceCommandAction.SkipToothOne,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth one skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth two"],
			ActionToPerform=VoiceCommandAction.SkipToothTwo,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth two skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth three"],
			ActionToPerform=VoiceCommandAction.SkipToothThree,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth three skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth four"],
			ActionToPerform=VoiceCommandAction.SkipToothFour,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth four skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth five"],
			ActionToPerform=VoiceCommandAction.SkipToothFive,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth five skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth six"],
			ActionToPerform=VoiceCommandAction.SkipToothSix,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth six skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth seven"],
			ActionToPerform=VoiceCommandAction.SkipToothSeven,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth seven skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth eight"],
			ActionToPerform=VoiceCommandAction.SkipToothEight,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth eight skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth nine"],
			ActionToPerform=VoiceCommandAction.SkipToothNine,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth nine skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth ten"],
			ActionToPerform=VoiceCommandAction.SkipToothTen,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth ten skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth eleven"],
			ActionToPerform=VoiceCommandAction.SkipToothEleven,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth eleven skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth twelve"],
			ActionToPerform=VoiceCommandAction.SkipToothTwelve,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth twelve skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth thirteen"],
			ActionToPerform=VoiceCommandAction.SkipToothThirteen,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth thirteen skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth fourteen"],
			ActionToPerform=VoiceCommandAction.SkipToothFourteen,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth fourteen skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth fifteen"],
			ActionToPerform=VoiceCommandAction.SkipToothFifteen,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth fifteen skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth sixteen"],
			ActionToPerform=VoiceCommandAction.SkipToothSixteen,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth sixteen skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth seventeen"],
			ActionToPerform=VoiceCommandAction.SkipToothSeventeen,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth seventeen skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth eighteen"],
			ActionToPerform=VoiceCommandAction.SkipToothEighteen,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth eighteen skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth nineteen"],
			ActionToPerform=VoiceCommandAction.SkipToothNineteen,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth nineteen skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth twenty"],
			ActionToPerform=VoiceCommandAction.SkipToothTwenty,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth twenty skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth twenty one"],
			ActionToPerform=VoiceCommandAction.SkipToothTwentyOne,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth twenty one skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth twenty two"],
			ActionToPerform=VoiceCommandAction.SkipToothTwentyTwo,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth twenty two skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth twenty three"],
			ActionToPerform=VoiceCommandAction.SkipToothTwentyThree,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth twenty three skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth twenty four"],
			ActionToPerform=VoiceCommandAction.SkipToothTwentyFour,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth twenty four skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth twenty five"],
			ActionToPerform=VoiceCommandAction.SkipToothTwentyFive,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth twenty five skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth twenty six"],
			ActionToPerform=VoiceCommandAction.SkipToothTwentySix,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth twenty six skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth twenty seven"],
			ActionToPerform=VoiceCommandAction.SkipToothTwentySeven,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth twenty seven skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth twenty eight"],
			ActionToPerform=VoiceCommandAction.SkipToothTwentyEight,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth twenty eight skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth twenty nine"],
			ActionToPerform=VoiceCommandAction.SkipToothTwentyNine,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth twenty nine skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth thirty"],
			ActionToPerform=VoiceCommandAction.SkipToothThirty,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth thirty skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth thirty one"],
			ActionToPerform=VoiceCommandAction.SkipToothThirtyOne,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth thirty one skipped"
		},
		new VoiceCommand {
			Commands= ["skip tooth thirty two"],
			ActionToPerform=VoiceCommandAction.SkipToothThirtyTwo,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth thirty two skipped"
		},
		new VoiceCommand {
			Commands=
			[
				"skip this tooth",
				"skip current tooth"
			],
			ActionToPerform=VoiceCommandAction.SkipCurrentTooth,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response="Tooth skipped"
		},
		#endregion PerioChart
		#region VoiceMsgBox
		new VoiceCommand {
			Commands=
			[
				"yes"
			],
			ActionToPerform=VoiceCommandAction.Yes,
			ListAreas= [VoiceCommandArea.VoiceMsgBox]
		},
		new VoiceCommand {
			Commands=
			[
				"no"
			],
			ActionToPerform=VoiceCommandAction.No,
			ListAreas= [VoiceCommandArea.VoiceMsgBox]
		},
		new VoiceCommand {
			Commands=
			[
				"okay"
			],
			ActionToPerform=VoiceCommandAction.Ok,
			ListAreas= [VoiceCommandArea.VoiceMsgBox]
		},
		new VoiceCommand {
			Commands=
			[
				"cancel"
			],
			ActionToPerform=VoiceCommandAction.Cancel,
			ListAreas= [VoiceCommandArea.VoiceMsgBox]
		},
		new VoiceCommand {
			Commands=
			[
				"resume path"
			],
			ActionToPerform=VoiceCommandAction.ResumePath,
			ListAreas= [VoiceCommandArea.PerioChart]
		},
		#endregion VoiceMsgBox
	};

	///<summary>Gets all the commands used for the given areas of the program.</summary>
	public static List<VoiceCommand> GetCommands(List<VoiceCommandArea> listAreas) {
		return _commands.FindAll(x => x.ListAreas.Any(y => listAreas.Contains(y)));
	}

	///<summary>Returns a new Go To Tooth voice command for the toothnum and voice command action passed in.</summary>
	private static VoiceCommand GoToToothCommandHelper(string toothNum,VoiceCommandAction action) {
		return new VoiceCommand {
			Commands=
			[
				$"go to tooth {toothNum.ToLower()}",
				$"go to {toothNum.ToLower()}",
				$"select tooth {toothNum.ToLower()}",
				$"select {toothNum.ToLower()}"
			],
			ActionToPerform=action,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response=$"Tooth {toothNum.ToLower()}"
		};
	}

	///<summary>Returns a new Go To Tooth voice command for the toothnum, position, and voice command action passed in.</summary>
	private static VoiceCommand GoToToothWithSurfaceXCommandHelper(string toothNum,string surface,VoiceCommandAction action) {
		return new VoiceCommand {
			Commands=
			[
				$"go to tooth {toothNum.ToLower()} {surface}",
				$"go to {toothNum.ToLower()} {surface}",
				$"select tooth {toothNum.ToLower()} {surface}",
				$"select {toothNum.ToLower()} {surface}"
			],
			ActionToPerform=action,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response=$"Tooth {toothNum.ToLower()} {surface}"
		};
	}

	///<summary>Returns a new Go To Tooth voice command for the toothnum, surface, position, and voice command action passed in.</summary>
	private static VoiceCommand GoToToothWithSurfaceCommandHelper(string toothNum,string surface,bool isFacial,VoiceCommandAction action) {
		var strPos=(isFacial ? "facial" : "lingual");
		return new VoiceCommand {
			Commands=
			[
				$"go to tooth {toothNum.ToLower()} {surface} {strPos}",
				$"go to {toothNum.ToLower()} {surface} {strPos}",
				$"select tooth {toothNum.ToLower()} {surface} {strPos}",
				$"select {toothNum.ToLower()} {surface} {strPos}",
				$"go to tooth {toothNum.ToLower()} {strPos} {surface}",
				$"go to {toothNum.ToLower()} {strPos} {surface}",
				$"select tooth {toothNum.ToLower()} {strPos} {surface}",
				$"select {toothNum.ToLower()} {strPos} {surface}"
			],
			ActionToPerform=action,
			ListAreas= [VoiceCommandArea.PerioChart],
			Response=$"Tooth {toothNum.ToLower()} {strPos} {surface}"
		};
	}
}