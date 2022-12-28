import anime from "animejs";
import domToImage from "dom-to-image";
import FileSaver from "file-saver";
import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Dispatch } from "redux";
import Character from "~/src/components/Character";
import StatGrader from "~/src/components/StatGrader";
import Floorlight from "~/src/graphics/floorlight";
import Lightbeam from "~/src/graphics/lightbeam";
import { IStoreState } from "~/src/interfaces/IStoreState";
import { characterActions } from "~/src/redux/character";
import { infoActions } from "~/src/redux/info";
import { statsActions } from "~/src/redux/stats";
import { uiActions } from "~/src/redux/ui";
import { decodeColourSet } from "~/src/utils/format-character-data";
import ArrowDown from "../EndScreen/arrow-down";
import {
  CharacterArea,
  CharacterCard,
  CtaArea,
  EndButton,
  EndScreenContainer,
  EndScreenWrapper,
  PublicUrl,
  QRCodeContainer,
  Spinner,
  SpinnerContainer,
  Stats,
} from "../EndScreen/styles";

const CharacterDisplay = () => {
  const qrUrl = window.location.href;
  const { gradedStats } = useSelector((store: IStoreState) => store.stats);
  const dispatch: Dispatch = useDispatch();
  useEffect(() => {
    const id = window.location.pathname.slice(1);
    fetch(`/api/get-character/${id}`)
      .then((res) => res.json())
      .then(({ companyInfo, stats, accessories = [], startedAt, completedAt, appearance }) => {
        // restore company info
        dispatch(infoActions.setBusinessCategory(companyInfo.businessCategory));
        dispatch(infoActions.setRole(companyInfo.role));
        dispatch(infoActions.setFunding(companyInfo.funding));
        dispatch(infoActions.setCompanySize(companyInfo.companySize));

        // restore stats
        for (const { level, name, category } of stats) {
          dispatch(statsActions.addStat({ name, category }));
          for (let i = 0; i < level; i++) {
            dispatch(statsActions.addStatPoint({ category, name, level }));
          }
        }

        // restore character
        accessories.map(characterActions.setAccessory).map(dispatch);
        dispatch(characterActions.setStartTime(startedAt));
        dispatch(characterActions.setShoes(appearance.shoes));
        dispatch(characterActions.setEyewear(appearance.eyewear));
        dispatch(characterActions.setHairStyle(appearance.hair.id));
        dispatch(characterActions.setHairColor(decodeColourSet(appearance.hair.color)));
        dispatch(characterActions.setFacialHair(appearance.facialHair.id));
        dispatch(characterActions.setFacialHairColor(decodeColourSet(appearance.facialHair.color)));

        if (appearance.skin) {
          dispatch(characterActions.setSkinColor(decodeColourSet(appearance.skin)));
        }
        if (appearance["t-shirt"]) {
          dispatch(characterActions.setTop("tshirt", decodeColourSet(appearance["t-shirt"])));
        }
        if (appearance["shirt"]) {
          dispatch(characterActions.setTop("shirt", decodeColourSet(appearance["shirt"])));
        }
        if (appearance["jacket"]) {
          dispatch(characterActions.setTop("jacket", decodeColourSet(appearance["jacket"])));
        }
        if (appearance["hoodie"]) {
          dispatch(characterActions.setTop("hoodie", decodeColourSet(appearance["hoodie"])));
        }
        if (appearance["bottom-clothes"]) {
          dispatch(
            characterActions.setBottom(
              appearance["bottom-clothes"].id,
              appearance["bottom-clothes"].color ? decodeColourSet(appearance["bottom-clothes"].color) : undefined
            )
          );
        }

        // go to the page
        dispatch(uiActions.setIdle(false));
        dispatch(uiActions.navigateTo(4));

        anime.set(".character-container", { filter: "brightness(1)" });
        anime.set(".skill-container, .start-button", { opacity: 1 });
      });
  }, [dispatch]);

  const downloadAvatar = async () => {
    const characterWrapper = document.getElementById("character-wrapper");

    if (!characterWrapper) {
      return;
    }

    const png = await domToImage.toPng(characterWrapper, { style: { transform: "none", opacity: 1 } });

    FileSaver.saveAs(png, "avatar.png");
  };

  const finish = () => {
    dispatch(uiActions.startOver());
  };

  useEffect(() => {
    anime.set("#character-area", { opacity: 0 });
    anime.set(".logo", { zIndex: -1 });
    anime.set("#character-card", {
      translateX: 750,
      translateY: -1000,
      scale: 1.2,
      zIndex: 99,
      opacity: 0,
    });
    anime.set("#character-area, .skill-container", { opacity: 1 });

    anime({
      targets: "span.flash",
      opacity: [1, 0],
      duration: 1000,
      easing: "linear",
    });

    anime({
      targets: ".view-quide-text",
      opacity: [0, 1],
      translateY: [30, 0],
      easing: "linear",
      duration: 500,
    });

    anime({
      targets: "#character-card",
      translateY: [-1000, -30],
      duration: 750,
      easing: "easeOutExpo",
      opacity: [0, 1],
      endDelay: 2000,

      complete: () => {
        anime({
          targets: "#character-card",
          duration: 750,
          easing: "easeOutExpo",

          complete: () => {
            anime.remove("#character-card");
            anime.set("#character-card", { translateY: -30, translateX: 750, scale: 1.2 });

            anime({
              targets: "#character-card",
              translateX: [750, 0],
              translateY: [-30, 0],
              scale: [1.2, 1],
              duration: 750,
              easing: "easeOutExpo",
            });

            anime({
              targets: ".dimmer",
              opacity: [1, 0],
              duration: 800,
              easing: "easeOutExpo",
              complete: () => {
                anime.set(".logo", { zIndex: 9 });
              },
            });
          },
        });
      },
    });
  }, []);

  return (
    <EndScreenWrapper>
      <p className="view-quide-text">The adventure continues!</p>
      <EndScreenContainer>
        <CharacterCard id="character-card">
          <CharacterArea id="character-area">
            <div className="lights">
              <Lightbeam className="lightbeam" />
              <Floorlight className="floorlight" />
            </div>
            <Character />
          </CharacterArea>
          <Stats>
            {gradedStats.map((stat, index: number) => (
              <StatGrader key={"statitem$$" + index} stat={stat} />
            ))}
          </Stats>
        </CharacterCard>
        <CtaArea>
          <h1>
            Mission accomplished
            <br />
            Startup Adventurer!
          </h1>
          <p className="cta-text">Scan QR code to get your avatar</p>
          <ArrowDown />
          {!qrUrl ? (
            <SpinnerContainer>
              <Spinner />
            </SpinnerContainer>
          ) : (
            <>
              <PublicUrl href={qrUrl} target="_blank">
                <QRCodeContainer>
                  <img src={`https://chart.googleapis.com/chart?chs=300x300&cht=qr&chl=${qrUrl}&choe=UTF-8`} alt="QR" />
                </QRCodeContainer>
              </PublicUrl>
            </>
          )}
          <EndButton onClick={downloadAvatar}>Download</EndButton>
          <EndButton onClick={finish}>Finish</EndButton>
        </CtaArea>
        <span className="dimmer" />
      </EndScreenContainer>
    </EndScreenWrapper>
  );
};

export default CharacterDisplay;
