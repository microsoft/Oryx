import React, { Fragment, useEffect } from "react";
import StartOver from "@/components/StartOver";
import CompanySizeRange from "@/components/CompanySizeRange";
import { infoActions } from "@/redux/info";
import { Dispatch } from "redux";
import Dropdown from "react-dropdown-aria";
import { useDispatch, useSelector } from "react-redux";
import { BasicInfoContainer, Title, InfoColumns, InfoColumn, FieldTitle, dropdownStyle } from "./styles";
import StepperFooter from "../../components/Stepper/StepperFooter";
import isEmpty from "lodash-es/isEmpty";
import { IStoreState } from "@/interfaces/IStoreState";
import anime from "animejs";
import arrow from "@/components/DropdownSelect/arrow-down.svg";

const businessCategories = [
	"Early Stage Startup",
	"Emerge-Tech Startup",
	"Fintech Startup",
	"Agritech Startup",
	"Travel and Tourism Startup",
	"Food and Drink Startup",
	"Social / Sustainable Startup",
	"Product and Manufacturing Startup",
	"eCommerce Startup",
	"Retail and Fashion Startup",
	"Tech Startup",
	"Medtech Startup",
	"Research Spinout / 3rd Level",
	"Venture Fund",
	"Not a Startup",
].map((cat) => {
	return { value: cat };
});

const roles = [
	"Business Development",
	"CEO",
	"CFO",
	"COO",
	"CTO",
	"Data Scientist",
	"Developer",
	"Head of Product",
	"Investor",
	"Sales",
	"Student",
	"Other",
].map((cat) => {
	return { value: cat };
});

const fundingOptions = [
	"Bootstrapping",
	"Pre-Seed",
	"Seed Capital",
	"Angel Investor Funding",
	"Venture Capital Financing",
	"Mezzanine Financing",
	"IPO",
	"Not a Startup",
].map((cat) => {
	return { value: cat };
});

const arrowRenderer = () => {
	return <img src={arrow} alt=""></img>;
};

const BasicInfo = () => {
	const dispatch: Dispatch = useDispatch();
	const { businessCategory, role, funding, companySize } = useSelector((store: IStoreState) => store.info);

	const selectBusiness = (item: string) => {
		dispatch(infoActions.setBusinessCategory(item));
	};

	const selectRole = (item: string) => {
		dispatch(infoActions.setRole(item));
	};

	const selectFunding = (item: string) => {
		dispatch(infoActions.setFunding(item));
	};

	const selectCompanySize = (item: string) => {
		dispatch(infoActions.setCompanySize(item));
	};

	const isAllFieldsFilled = (): boolean => {
		return !isEmpty(businessCategory) && !isEmpty(role) && !isEmpty(funding) && !isEmpty(companySize);
	};

	const removeInlineStyles = () => {
		const cols = document.querySelectorAll(".animated");
		Array.from(cols).forEach((col) => col.removeAttribute("style"));
	};

	const beforeNext = async () => {
		anime({
			targets: ".view-quide-text",
			opacity: [1, 0],
			translateY: [0, -30],
			easing: "linear",
			duration: 350,
		});
		await anime({
			targets: ".info-container",
			opacity: [1, 0],
			duration: 350,
			easing: "linear",
		}).finished;
	};

	useEffect(() => {
		anime.set(".info-container", { opacity: 0 });
		anime.set(".stepper-nav", { opacity: 0 });

		anime({
			targets: ".view-quide-text",
			opacity: [0, 1],
			translateY: [30, 0],
			easing: "linear",
			duration: 500,
		});

		const tl = anime.timeline({
			complete: () => removeInlineStyles(),
			easing: "easeOutExpo",
		});

		tl.add({
			targets: ".info-container",
			opacity: 1,
			duration: 200,
		});

		tl.add({
			targets: ".animated",
			opacity: [0, 1],
			translateY: ["50px", "0px"],
			duration: 250,
			delay: anime.stagger(100),
		});

		tl.add({
			targets: ".stepper-nav",
			opacity: [0, 1],
			duration: 500,
		});
	}, []);

	return (
		<Fragment>
			<p className="view-quide-text">Tell us a little about your startup!</p>
			<StartOver />
			<BasicInfoContainer className="info-container">
				<Title className="animated">Who are you?</Title>
				<InfoColumns>
					<InfoColumn className="animated">
						<FieldTitle>Business category</FieldTitle>
						<Dropdown
							placeholder="Select"
							options={businessCategories}
							setSelected={selectBusiness}
							selectedOption={businessCategory}
							arrowRenderer={arrowRenderer}
							style={dropdownStyle}
						/>
					</InfoColumn>
					<InfoColumn className="animated">
						<FieldTitle>Your role</FieldTitle>
						<Dropdown
							placeholder="Select"
							options={roles}
							setSelected={selectRole}
							selectedOption={role}
							arrowRenderer={arrowRenderer}
							style={dropdownStyle}
						/>
					</InfoColumn>
				</InfoColumns>
				<InfoColumns>
					<InfoColumn className="animated">
						<FieldTitle>Company size ({companySize})</FieldTitle>
						<CompanySizeRange onChange={selectCompanySize} />
					</InfoColumn>
					<InfoColumn className="animated">
						<FieldTitle>Funding</FieldTitle>
						<Dropdown
							options={fundingOptions}
							setSelected={selectFunding}
							selectedOption={funding}
							placeholder="Select"
							arrowRenderer={arrowRenderer}
							style={dropdownStyle}
						/>
					</InfoColumn>
				</InfoColumns>
			</BasicInfoContainer>
			<StepperFooter
				nextHtml="Continue to build character"
				nextDisabled={!isAllFieldsFilled()}
				beforeNext={beforeNext}
			/>
		</Fragment>
	);
};

export default BasicInfo;
